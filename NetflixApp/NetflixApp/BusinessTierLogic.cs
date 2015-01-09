// // Netflix Database Application using N-Tier Design. 
// // Bryan Mendoza 



//
// BusinessTier:  business logic, acting as interface between UI and data store.
//

using System;
using System.Collections.Generic;
using System.Data;


namespace BusinessTier
{

  //
  // Business:
  //
  public class Business
  {
    //
    // Fields:
    //
    private string _DBFile;
    private DataAccessTier.Data datatier;


    //
    // Constructor:
    //
    public Business(string DatabaseFilename)
    {
      _DBFile = DatabaseFilename;

      datatier = new DataAccessTier.Data(DatabaseFilename);
    }


    //
    // TestConnection:
    //
    // Returns true if we can establish a connection to the database, false if not.
    //
    public bool TestConnection()
    {
      return datatier.TestConnection();
    }


    //
    // GetMovie:
    //
    // Retrieves Movie object based on MOVIE ID; returns null if movie is not
    // found.
    //
    public Movie GetMovie(int MovieID)
    {

        string sql = string.Format("SELECT MovieName FROM Movies WHERE MovieID={0};",MovieID);
        object result = datatier.ExecuteScalarQuery(sql);

        if (result == null || result.ToString() == "")
        {
            return null;
        }
        else
        {
            return new Movie(MovieID, result.ToString());
        }
      
    }


    //
    // GetMovie:
    //
    // Retrieves Movie object based on MOVIE NAME; returns null if movie is not
    // found.
    //
    public Movie GetMovie(string MovieName)
    {

        string sql = String.Format("SELECT MovieID FROM Movies WHERE MovieName = '{0}';",MovieName);
        object result = datatier.ExecuteScalarQuery(sql);

        if (result == null || result.ToString() == "")
        {
            return null;
        }
        else
        {
            return new Movie(Convert.ToInt32(result.ToString()),MovieName);
        }
      
    }


    //
    // AddMovie:
    //
    // Adds the movie, returning a Movie object containing the name and the 
    // movie's id.  If the add failed, null is returned.
    //
    public Movie AddMovie(string MovieName)
    {

        string sql = String.Format(@"INSERT INTO Movies(MovieName) Values('{0}'); 
                                    SELECT MovieID FROM Movies WHERE MovieName = '{0}';", MovieName);

        object result = datatier.ExecuteScalarQuery(sql);

        if (result == null || result.ToString() == "")
        {
            return null;
        }
        else
        {
            return new Movie(Convert.ToInt32(result.ToString()), MovieName);
        }
 
    }


    //
    // AddReview:
    //
    // Adds review based on MOVIE ID, returning a Review object containing
    // the review, review's id, etc.  If the add failed, null is returned.
    //
    public Review AddReview(int MovieID, int UserID, int Rating)
    {

      string sql = String.Format(@"INSERT INTO Reviews(MovieID,UserID,Rating) Values({0},{1},{2}); 
                                    SELECT MAX(ReviewID) FROM Reviews;", MovieID,UserID,Rating);

      object result = datatier.ExecuteScalarQuery(sql);

      if (result == null || result.ToString() == "")
      {
          return null;
      }
      else
      {
          return new Review(Convert.ToInt32(result.ToString()), MovieID, UserID,Rating);
      }
      
    }


    //
    // GetMovieDetail:
    //
    // Given a MOVIE ID, returns detailed information about this movie --- all
    // the reviews, the total number of reviews, average rating, etc.  If the 
    // movie cannot be found, null is returned.
    //
    public MovieDetail GetMovieDetail(int MovieID)
    {

        object avg, numRevs;

        Movie movie = GetMovie(MovieID);

        if (movie == null)
        {
            return null;
        }

        //Get average into object
        string sql = String.Format(@"SELECT ROUND(AVG(CAST(Rating AS Float)),4) FROM Reviews WHERE MovieID = {0};", MovieID);
        avg = datatier.ExecuteScalarQuery(sql);

        //If average is null then return empty details
        if (avg == null || avg.ToString() == "")
        {
            return new MovieDetail(movie, 0, 0, new List<Review>()); 
        }
        
        //Get number of reviews
        sql = String.Format(@"SELECT COUNT(*) FROM Reviews WHERE MovieID = {0};", MovieID);
        numRevs = datatier.ExecuteScalarQuery(sql);

        //Get reviews information to store in review objects
        sql = String.Format(@"SELECT * FROM Reviews WHERE MovieID = {0} ORDER BY Rating DESC, UserID ASC;", MovieID);

        DataSet ds = datatier.ExecuteNonScalarQuery(sql);

        DataTable dt = ds.Tables["Table"];

        List<Review> reviews = new List<Review>(); 

        foreach (DataRow row in dt.Rows)
        {
            int revID = Convert.ToInt32(row["ReviewID"].ToString());
            int movID = Convert.ToInt32(row["MovieID"].ToString());
            int uID = Convert.ToInt32(row["UserID"].ToString());
            int rate = Convert.ToInt32(row["Rating"].ToString());
            reviews.Add(new Review(revID,movID,uID,rate));
        }

        return new MovieDetail(movie, Convert.ToDouble(avg.ToString()), Convert.ToInt32(numRevs.ToString()),reviews);
    }


    //
    // GetUserDetail:
    //
    // Given a USER ID, returns detailed information about this user --- all
    // the reviews submitted by this user, the total number of reviews, average 
    // rating given, etc.  If the user cannot be found, null is returned.
    //
    public UserDetail GetUserDetail(int UserID)
    {
        object avg, numRevs, result;

        //Check if user is in DataBase

        string sql = String.Format(@"SELECT UserID from reviews WHERE UserID = {0} GROUP BY UserID;", UserID);

        result = datatier.ExecuteScalarQuery(sql);

        if (result == null || result.ToString() == "")
        {
            return null;
        }
        
        //Create user object
        User user = new User(UserID);

        //Get average object
        sql = String.Format(@"SELECT ROUND(AVG(CAST(Rating AS Float)),4) FROM Reviews WHERE UserID = {0};", UserID);
        avg = datatier.ExecuteScalarQuery(sql);

        //Get number of reviews
        sql = String.Format(@"SELECT COUNT(*) FROM Reviews WHERE UserID = {0};", UserID);
        numRevs = datatier.ExecuteScalarQuery(sql);

        //Get review information of user
        sql = String.Format(@"SELECT * FROM Reviews WHERE UserID = {0} ORDER BY Rating DESC, MovieID ASC;", UserID);

        DataSet ds = datatier.ExecuteNonScalarQuery(sql);

        DataTable dt = ds.Tables["Table"];

        List<Review> reviews = new List<Review>();

        //Convert object and add new Review to list
        foreach (DataRow row in dt.Rows)
        {
            int revID = Convert.ToInt32(row["ReviewID"].ToString());
            int movID = Convert.ToInt32(row["MovieID"].ToString());
            int uID = Convert.ToInt32(row["UserID"].ToString());
            int rate = Convert.ToInt32(row["Rating"].ToString());
            reviews.Add(new Review(revID, movID, uID, rate));
        }

        return new UserDetail(user,Convert.ToDouble(avg.ToString()), Convert.ToInt32(numRevs.ToString()), reviews);
    }


    //
    // GetTopMoviesByAvgRating:
    //
    // Returns the top N movies in descending order by average rating.  If two
    // movies have the same rating, the movies are presented in ascending order
    // by name.  If N < 1, an EMPTY LIST is returned.
    //
    public IReadOnlyList<Movie> GetTopMoviesByAvgRating(int N)
    {
      List<Movie> movies = new List<Movie>();

      if (N < 1)
      {
          return movies;
      }

      string sql = String.Format(@"SELECT TOP {0} MovieName, AvgRating From Movies
                                INNER JOIN
                                (
                                    SELECT MovieID, ROUND(AVG(CAST(Rating AS Float)),4) as AvgRating
                                    FROM Reviews
                                    GROUP BY MovieID

                                )TEMP
                                ON TEMP.MovieID = Movies.MovieID
                                ORDER BY AvgRating DESC, MovieName ASC;",N);

      DataSet ds = datatier.ExecuteNonScalarQuery(sql);

      DataTable dt = ds.Tables["Table"];

      foreach (DataRow row in dt.Rows)
      {
          string movName = row["MovieName"].ToString();
          movName = movName.Replace("'","''");
          Movie movie = GetMovie(movName);
          movies.Add(movie);
      }
      
      return movies;
    }


    //
    // GetTopMoviesByNumReviews
    //
    // Returns the top N movies in descending order by number of reviews.  If two
    // movies have the same number of reviews, the movies are presented in ascending
    // order by name.  If N < 1, an EMPTY LIST is returned.
    //
    public IReadOnlyList<Movie> GetTopMoviesByNumReviews(int N)
    {
      List<Movie> movies = new List<Movie>();


      if (N < 1)
      {
          return movies;
      }

      string sql = String.Format(@"SELECT TOP {0} MovieName, numReviews FROM Movies
                                INNER JOIN
                                (
	                                SELECT MovieID, COUNT(MovieID) AS numReviews
                                    FROM Reviews
                                    GROUP BY MovieID
                                )TEMP
                                ON TEMP.MovieID = Movies.MovieID
                                ORDER BY numReviews DESC, MovieName ASC; ", N);

      DataSet ds = datatier.ExecuteNonScalarQuery(sql);

      DataTable dt = ds.Tables["Table"];

      foreach (DataRow row in dt.Rows)
      {
          string movName = row["MovieName"].ToString();
          movName = movName.Replace("'", "''");
          //Use GetMovie to return movie objects by passing in movie name from table
          Movie movie = GetMovie(movName);
          movies.Add(movie);
      }

      return movies;

    }


    //
    // GetTopUsersByNumReviews
    //
    // Returns the top N users in descending order by number of reviews.  If two
    // users have the same number of reviews, the users are presented in ascending
    // order by user id.  If N < 1, an EMPTY LIST is returned.
    //
    public IReadOnlyList<User> GetTopUsersByNumReviews(int N)
    {
      List<User> users = new List<User>();

      if (N < 1)
      {
          return users;
      }  

      string sql = String.Format(@"SELECT TOP {0} UserID, COUNT(UserID) AS NumberOfReviews
                                FROM Reviews
                                GROUP BY UserID
                                ORDER BY NumberOfReviews DESC, UserID ASC;", N);

      DataSet ds = datatier.ExecuteNonScalarQuery(sql);

      DataTable dt = ds.Tables["Table"];

      foreach (DataRow row in dt.Rows)
      {
          int uID = Convert.ToInt32(row["UserID"].ToString());
          users.Add(new User(uID));
      }
      
      return users;
    }


    //Step 4 in Homework
    public IReadOnlyList<Movie> GetMovies()
    {
        List<Movie> movies = new List<Movie>();

        string sql = String.Format(@"SELECT MovieName FROM Movies ORDER BY MovieName ASC;");

        DataSet ds = datatier.ExecuteNonScalarQuery(sql);

        if (ds == null || ds.ToString() == "")
        {
            return movies;
        }
        else
        {

            DataTable dt = ds.Tables["Table"];

            foreach (DataRow row in dt.Rows)
            {
                string movName = row["MovieName"].ToString();
                movName = movName.Replace("'", "''");
                Movie movie = GetMovie(movName);
                movies.Add(movie);
            }

            return movies;
        }
    }

  }//class

}//namespace
