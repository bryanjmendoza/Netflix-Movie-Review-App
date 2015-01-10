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
        //Create query 
        string sql = string.Format("SELECT MovieName FROM Movies WHERE MovieID={0};",MovieID);

        //Using Data tier execute a scalar query and return object
        object result = datatier.ExecuteScalarQuery(sql);

        //If we get nothing from the Database
        if (result == null || result.ToString() == "")
        {
            return null;
        }
        else
        {
            //Otherwise create new Movie object and return
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
        //Create query
        string sql = String.Format("SELECT MovieID FROM Movies WHERE MovieName = '{0}';",MovieName);

        //Execute Query
        object result = datatier.ExecuteScalarQuery(sql);

        //If movie not found
        if (result == null || result.ToString() == "")
        {
            return null;
        }
        else
        {
            //Create movie object and return
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

        //Create query to insert and retrieve newly created ID for movie
        string sql = String.Format(@"INSERT INTO Movies(MovieName) Values('{0}'); 
                                    SELECT MovieID FROM Movies WHERE MovieName = '{0}';", MovieName);

        //Execute query
        object result = datatier.ExecuteScalarQuery(sql);

        //If Movie could not be added
        if (result == null || result.ToString() == "")
        {
            return null;
        }
        else
        {
            //Otherwise return newly inserted movie object
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

      //Create query to insert review into DB and also returns the newly created ID
      string sql = String.Format(@"INSERT INTO Reviews(MovieID,UserID,Rating) Values({0},{1},{2}); 
                                    SELECT MAX(ReviewID) FROM Reviews;", MovieID,UserID,Rating);

      //Execute query
      object result = datatier.ExecuteScalarQuery(sql);

      //If review could not be added
      if (result == null || result.ToString() == "")
      {
          return null;
      }
      else
      {
          //Otherwise create new Review object
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
        //Objects for number of reviews and average rating
        object avg, numRevs;

        //get movie object given movie ID
        Movie movie = GetMovie(MovieID);

        //If movie does not exist then return null
        if (movie == null)
        {
            return null;
        }

        //Get average into object by creating a sql string and executing the query
        string sql = String.Format(@"SELECT ROUND(AVG(CAST(Rating AS Float)),4) FROM Reviews WHERE MovieID = {0};", MovieID);
        avg = datatier.ExecuteScalarQuery(sql);

        //If avg object is null then return empty details
        if (avg == null || avg.ToString() == "")
        {
            return new MovieDetail(movie, 0, 0, new List<Review>()); 
        }
        
        //Get number of reviews by creating new query string to get number of reviews
        sql = String.Format(@"SELECT COUNT(*) FROM Reviews WHERE MovieID = {0};", MovieID);
        numRevs = datatier.ExecuteScalarQuery(sql);

        //Get all reviews from this movie to store in review objects
        //Create new query string
        sql = String.Format(@"SELECT * FROM Reviews WHERE MovieID = {0} ORDER BY Rating DESC, UserID ASC;", MovieID);

        //execute query string and get table
        DataSet ds = datatier.ExecuteNonScalarQuery(sql);

        DataTable dt = ds.Tables["Table"];
        
        //Create empty list of review objects
        List<Review> reviews = new List<Review>(); 

        //For each row in table
        foreach (DataRow row in dt.Rows)
        {
            //Get review Id
            int revID = Convert.ToInt32(row["ReviewID"].ToString());

            //Get movie ID
            int movID = Convert.ToInt32(row["MovieID"].ToString());

            //Get user ID
            int uID = Convert.ToInt32(row["UserID"].ToString());

            //Get rating
            int rate = Convert.ToInt32(row["Rating"].ToString());

            //Create review object and add to list
            reviews.Add(new Review(revID,movID,uID,rate));
        }

        //Create MovieDetail object and return it
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
        //Create new query string
        string sql = String.Format(@"SELECT UserID from reviews WHERE UserID = {0} GROUP BY UserID;", UserID);

        //Execute sql string
        result = datatier.ExecuteScalarQuery(sql);

        //If user not found return null
        if (result == null || result.ToString() == "")
        {
            return null;
        }
        
        //Otherwise Create user object
        User user = new User(UserID);

        //Get average rating of User by creating new query string
        sql = String.Format(@"SELECT ROUND(AVG(CAST(Rating AS Float)),4) FROM Reviews WHERE UserID = {0};", UserID);
        avg = datatier.ExecuteScalarQuery(sql);

        //Get number of reviews of User by creating new query string
        sql = String.Format(@"SELECT COUNT(*) FROM Reviews WHERE UserID = {0};", UserID);
        numRevs = datatier.ExecuteScalarQuery(sql);

        //Get all reviews by this user by creating new query string
        sql = String.Format(@"SELECT * FROM Reviews WHERE UserID = {0} ORDER BY Rating DESC, MovieID ASC;", UserID);

        //User reviews will be in table
        DataSet ds = datatier.ExecuteNonScalarQuery(sql);

        DataTable dt = ds.Tables["Table"];

        //Create empty list of Review objects
        List<Review> reviews = new List<Review>();

        //For each row of table
        //Convert row object to string and add new Review to list
        foreach (DataRow row in dt.Rows)
        {
            int revID = Convert.ToInt32(row["ReviewID"].ToString());
            int movID = Convert.ToInt32(row["MovieID"].ToString());
            int uID = Convert.ToInt32(row["UserID"].ToString());
            int rate = Convert.ToInt32(row["Rating"].ToString());

            //Create new review object and add to list
            reviews.Add(new Review(revID, movID, uID, rate));
        }

        //Create UserDetail object and return it
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
       //Create empty List of Movie objects 
      List<Movie> movies = new List<Movie>();

      //If input is 0 or negative then return empty list
      if (N < 1)
      {
          return movies;
      }

      //Create query string to inner join Movie and Review Table
      string sql = String.Format(@"SELECT TOP {0} MovieName, AvgRating From Movies
                                INNER JOIN
                                (
                                    SELECT MovieID, ROUND(AVG(CAST(Rating AS Float)),4) as AvgRating
                                    FROM Reviews
                                    GROUP BY MovieID

                                )TEMP
                                ON TEMP.MovieID = Movies.MovieID
                                ORDER BY AvgRating DESC, MovieName ASC;",N);

      //Execute query and return table
      DataSet ds = datatier.ExecuteNonScalarQuery(sql);

      DataTable dt = ds.Tables["Table"];

      //For each row of table
      foreach (DataRow row in dt.Rows)
      {
          //Get movie name
          string movName = row["MovieName"].ToString();
          movName = movName.Replace("'","''");

          //Get movie object
          Movie movie = GetMovie(movName);

          //Add movie object to list
          movies.Add(movie);
      }
      
      //Return list of movie objects
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
      
      //Create empty List of Movie objects
      List<Movie> movies = new List<Movie>();

      //If input is 0 or negative then return empty list
      if (N < 1)
      {
          return movies;
      }

      //Create query string to inner join the Movie and Review Tables
      string sql = String.Format(@"SELECT TOP {0} MovieName, numReviews FROM Movies
                                INNER JOIN
                                (
	                                SELECT MovieID, COUNT(MovieID) AS numReviews
                                    FROM Reviews
                                    GROUP BY MovieID
                                )TEMP
                                ON TEMP.MovieID = Movies.MovieID
                                ORDER BY numReviews DESC, MovieName ASC; ", N);

      //Execute query
      DataSet ds = datatier.ExecuteNonScalarQuery(sql);

      //Get table
      DataTable dt = ds.Tables["Table"];

      //For each row of table
      foreach (DataRow row in dt.Rows)
      {
          //Get movie name
          string movName = row["MovieName"].ToString();
          movName = movName.Replace("'", "''");

          //Use GetMovie to return movie objects by passing in movie name from table
          Movie movie = GetMovie(movName);

          //Add movie object to list
          movies.Add(movie);
      }

      //Return list of Movies
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
        
      //Create empty List of User objects
      List<User> users = new List<User>();

      //If 0 or negative number given as input return empty list
      if (N < 1)
      {
          return users;
      }  

      //Create query to get UserIds and Total reviews from user
      //The IDs are in descending order by number of reviews for each user
      string sql = String.Format(@"SELECT TOP {0} UserID, COUNT(UserID) AS NumberOfReviews
                                FROM Reviews
                                GROUP BY UserID
                                ORDER BY NumberOfReviews DESC, UserID ASC;", N);

      //Execute non scalar query by calling datatier
      DataSet ds = datatier.ExecuteNonScalarQuery(sql);

      //Get tables returned by datatier
      DataTable dt = ds.Tables["Table"];

      //For each row in table
      foreach (DataRow row in dt.Rows)
      {
          //Get the user ID
          int uID = Convert.ToInt32(row["UserID"].ToString());

          //Add new user object to lisr
          users.Add(new User(uID));
      }
      
      //Return list of User objects
      return users;
    }


    //Get all movies in list
    public IReadOnlyList<Movie> GetMovies()
    {
        //Create empty List of movies
        List<Movie> movies = new List<Movie>();

        //Create query
        string sql = String.Format(@"SELECT MovieName FROM Movies ORDER BY MovieName ASC;");

        //Execute non scalar Query
        DataSet ds = datatier.ExecuteNonScalarQuery(sql);

        //If no movies, then return empty movie list
        if (ds == null || ds.ToString() == "")
        {
            return movies;
        }
        else
        {
            //Otherwise parse the table returned by datatier
            DataTable dt = ds.Tables["Table"];

            //For each row of the table
            foreach (DataRow row in dt.Rows)
            {
                //get the movie name
                string movName = row["MovieName"].ToString();

                //Get movie object
                movName = movName.Replace("'", "''");
                Movie movie = GetMovie(movName);

                //Add movie object to list
                movies.Add(movie);
            }

            //Return Movie List
            return movies;
        }
    }

  }//class

}//namespace
