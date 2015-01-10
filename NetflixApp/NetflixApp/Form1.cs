using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


// // Netflix Database Application using N-Tier Design. 
// // Bryan Mendoza 



namespace NetflixApp
{
    public partial class Form1 : Form
    {
        //
        // Class members:
        //
        private Random RandomNumberGenerator;
        private BusinessTier.Business BT;

        //
        // Constructor:
        //
        public Form1()
        {
            InitializeComponent();

            RandomNumberGenerator = new Random();
            BT = new BusinessTier.Business("netflix.mdf");

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //
        // Test Connection:
        //
        private void cmdConnect_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            //Test database connection
            bool status = BT.TestConnection();

            if (status)
            {
                this.listBox1.Items.Add("Connection successful.");
            }
            else
            {
                this.listBox1.Items.Add("Connection failed.");
            }
          
        }

        //Check if user enters positive integer
        private Boolean parseInput_Int(string input)
        {
            //Does user enter an integer
            int N;

            try
            {
                N = System.Convert.ToInt32(input);
            }
            catch
            {
                this.listBox1.Items.Add("Please enter integer.");
                return false;
            }

            //If integer is less than 0
            if (N < 0)
            {
                this.listBox1.Items.Add("Please enter integer greater than 0.");
                return false;
            }
            //Good input
            else
            {
                return true;
            }
        }

        //
        // Get Movie Name:  from id...
        //
        private void cmdGetMovieName_Click(object sender, EventArgs e)
        {
            //Clear list box on App
            listBox1.Items.Clear();

            //Does user enter an integer?
            if (!parseInput_Int(txtMovieID.Text))
            {
                return;
            }

            //Convert input into integer and get movie object
            //from BusinessTier

            int N = System.Convert.ToInt32(txtMovieID.Text);

            BusinessTier.Movie movie = BT.GetMovie(N);

            //If movie is not in database
            if (movie == null)
            {
                this.listBox1.Items.Add(String.Format("Movie ID {0} not found.",N)); 
            }
            else
            {
                this.listBox1.Items.Add(movie.MovieName); 
            }

        }

        //
        // Get Movie Reviews: from id...
        //
        private void cmdGetMovieReviews_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            //Does user enter an integer?
            if (!parseInput_Int(txtMovieID.Text))
            {
                return;
            }

            int N = System.Convert.ToInt32(txtMovieID.Text);

            //Get MovieDetail Object from Business Tier
            BusinessTier.MovieDetail movieDetail = BT.GetMovieDetail(N);

            //If no details for movie
            if (movieDetail == null)
            {
                this.listBox1.Items.Add(String.Format("Movie ID {0} not found.", N));
                return;
            }
          
            //Otherwise Get the user reviews from MovieDetail object.
            IReadOnlyList<BusinessTier.Review> reviews = movieDetail.Reviews;

            //Get review count
            int numReviews = reviews.Count;

            //If no reviews
            if (numReviews == 0)
            {
                this.listBox1.Items.Add("No reviews for this movie yet.!!");
                return;
            }
            
            //Parse each review in the list
            foreach(BusinessTier.Review review in reviews){

                string msg = string.Format("User {0}  :  Rating {1}",
                    review.UserID,
                    review.Rating);

                //Diplay each review for the movie
                this.listBox1.Items.Add(msg);

            }
            
            
        }

        //
        // Get Average Rating:
        //
        private void cmdAvgRating_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            
            //
            // Compute average rating:
            //
            // escape any single ' in the string:
            string name = txtRatingsMovieName.Text;
            name = name.Replace("'", "''");  

            //Get movie object from name input string
            BusinessTier.Movie movie = BT.GetMovie(name);

            //If movie does not exist
            if (movie == null)
            {
                this.listBox1.Items.Add("Movie not found!!");
                return;
            }
            else
            {
                //Get MovieDetail object from BusinessTier using the movie ID
                BusinessTier.MovieDetail details = BT.GetMovieDetail(movie.MovieID);

                //Display the average rating from Movie Detail object
                listBox1.Items.Add("Average rating: " + details.AvgRating);
            }
            
        }

        //
        // Get Each Rating:
        //
        private void cmdEachRating_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            // escape any single ' in the string:
            string name = txtRatingsMovieName.Text;
            name = name.Replace("'", "''"); 

            //Get Movie Object from textbox input string
            BusinessTier.Movie movie = BT.GetMovie(name);

            //If movie does not exists
            if (movie == null)
            {
                this.listBox1.Items.Add("Movie not found!!");
                return;
            }
            else
            {

                //Get individual ratings by using counters
                int rate1 = 0, rate2 = 0, rate3 = 0, rate4 = 0, rate5 = 0;

                //Get the review objects list from the movie details function
                BusinessTier.MovieDetail details = BT.GetMovieDetail(movie.MovieID);
                IReadOnlyList<BusinessTier.Review> reviews = details.Reviews;

                //Count up the reviews by parsing every review for 
                // the movie
                foreach (BusinessTier.Review review in reviews)
                {
                    if (review.Rating == 1)
                    {
                        rate1++;
                    }
                    else if (review.Rating == 2)
                    {
                        rate2++;
                    }
                    else if (review.Rating == 3)
                    {
                        rate3++;
                    }
                    else if (review.Rating == 4)
                    {
                        rate4++;
                    }
                    else if (review.Rating == 5){
                        rate5++;
                    }
                }

                //Display the reviews
                listBox1.Items.Add("5: " + rate5);
                listBox1.Items.Add("4: " + rate4);
                listBox1.Items.Add("3: " + rate3);
                listBox1.Items.Add("2: " + rate2);
                listBox1.Items.Add("1: " + rate1);
                listBox1.Items.Add("Total: " + details.NumReviews);
            }
            

   
        }

        //
        // Add movie to DataBase:
        //
        private void cmdInsertMovie_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            string text;

            //Modify string for injection prevention

            text = this.txtInsertMovieName.Text;

            text = text.Replace("'", "''");

            //Is movie already there?
            BusinessTier.Movie checkMovie = BT.GetMovie(text);

            //If movie already exists then don't add
            if (checkMovie != null)
            {
                this.listBox1.Items.Add(String.Format("{0} already exists.", checkMovie.MovieName));
                return;
            }

            //Otherwise add movie
            BusinessTier.Movie movie = BT.AddMovie(text);

            //If insert failed
            if (movie == null)
            {
                this.listBox1.Items.Add("Insert Failed?!!!");    
            }
            else
            {
                //Movie was added
                this.listBox1.Items.Add("Success, movie added.");

                //Display new ID to list box
                this.listBox1.Items.Add("Movie ID: " + movie.MovieID.ToString());
            }
        }

        //Rating scroll tool
        private void tbarRating_Scroll(object sender, EventArgs e)
        {
            lblRating.Text = tbarRating.Value.ToString();
        }

        //
        // Add Review:
        //
        private void cmdInsertReview_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            // escape any single ' in the string:
            string name = txtInsertMovieName.Text;
            name = name.Replace("'", "''"); 

            //Check if movie we want to review exists
            BusinessTier.Movie movie = BT.GetMovie(name);

            //If movie does not exists
            if (movie == null)
            {
                this.listBox1.Items.Add("Movie not found.");    
            }
            else
            {
               //Convert rating string to int
                int rating = Convert.ToInt32(lblRating.Text);
                int movieid = movie.MovieID;

                //Assign the user a random ID number
                int userid = RandomNumberGenerator.Next(100000, 999999);  // 6-digit user ids:

                //Call our AddReviews and display results
                BusinessTier.Review review = BT.AddReview(movieid, userid, rating);

                //If review could not be added
                if (review == null)
                {
                    this.listBox1.Items.Add("Review not added.");
                }
                else
                {
                    //Review was added successfully
                    this.listBox1.Items.Add("Success, review added.");
                    this.listBox1.Items.Add("Review ID: " + review.ReviewID.ToString());
                }
            }
         

        }

        //
        // Top N Movies by Average Rating:
        //
        private void cmdTopMoviesByAvgRating_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            //Does user enter an integer?
            if (!parseInput_Int(txtTopN.Text))
            {
                return;
            }

            int N = System.Convert.ToInt32(txtTopN.Text);

            //Get movie objects by calling the Business Tiers appropriate method
            IReadOnlyList<BusinessTier.Movie> movies = BT.GetTopMoviesByAvgRating(N);

            //Parse each movie in the list
            foreach(BusinessTier.Movie movie in movies)
            {
                //Get movie detail object for every movie to get average
                BusinessTier.MovieDetail detail = BT.GetMovieDetail(movie.MovieID);

                //Convert to string
                string msg = string.Format("{0}: AVG={1}",
                    movie.MovieName,
                    detail.AvgRating);

                //Display movie name along with average rating
                this.listBox1.Items.Add(msg);
                
            }

        }

        //
        // Top N Movies by # of reviews:
        //
        private void cmdTopMoviesByNumReviews_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            //Does user enter an integer?
            if (!parseInput_Int(txtTopN.Text))
            {
                return;
            }

            int N = System.Convert.ToInt32(txtTopN.Text);

            //Get movie objects by calling BusinessTier method
            IReadOnlyList<BusinessTier.Movie> movies = BT.GetTopMoviesByNumReviews(N);

            //For each movie in list
            foreach (BusinessTier.Movie movie in movies)
            {
                //Call movie details to get number of reviews
                BusinessTier.MovieDetail movieDetail = BT.GetMovieDetail(movie.MovieID);

                //Get the number of reviews and movie name and convert to string
                string msg = string.Format("{0} : {1}",
                     movie.MovieName,
                     movieDetail.NumReviews);

                //Display results for each movie
                this.listBox1.Items.Add(msg);

            }

            
        }

        //
        // Top N Users by # of reviews:
        //
        private void cmdTopUsers_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            //Does user enter an integer?
            if (!parseInput_Int(txtTopN.Text))
            {
                return;
            }

            int N = System.Convert.ToInt32(txtTopN.Text);

            //Get user object from BusinessTier
            IReadOnlyList<BusinessTier.User> users = BT.GetTopUsersByNumReviews(N);

            //For each user object
            foreach (BusinessTier.User user in users)
            {
                //Call user details to get user number of reviews
                BusinessTier.UserDetail userDetail = BT.GetUserDetail(user.UserID);

                //Convert user number of reviews into string
                string msg = string.Format("{0} : {1}",
                     user.UserID,
                     userDetail.NumReviews);

                //Display results
                this.listBox1.Items.Add(msg);

            }

           
        }

        //Display every movie in DataBase
        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            //Get all movie objects
            IReadOnlyList<BusinessTier.Movie> movies = BT.GetMovies();

            //If no movies
            if (movies.Count == 0)
            {
                this.listBox1.Items.Add("No Movies.");
            }
            else
            {
                //Display each movie name
                foreach (BusinessTier.Movie movie in movies)
                {
                    this.listBox1.Items.Add(movie.MovieName);
                }
            }

        }

    }//class
}//namespace
