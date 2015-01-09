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

            if (N < 0)
            {
                this.listBox1.Items.Add("Please enter integer greater than 0.");
                return false;
            }
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
            listBox1.Items.Clear();

            //Does user enter an integer?
            if (!parseInput_Int(txtMovieID.Text))
            {
                return;
            }

            int N = System.Convert.ToInt32(txtMovieID.Text);

            BusinessTier.Movie movie = BT.GetMovie(N);

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

            BusinessTier.MovieDetail movieDetail = BT.GetMovieDetail(N);

            if (movieDetail == null)
            {
                this.listBox1.Items.Add(String.Format("Movie ID {0} not found.", N));
                return;
            }
          
            //Get the user reviews
            IReadOnlyList<BusinessTier.Review> reviews = movieDetail.Reviews;

            int numReviews = reviews.Count;

            if (numReviews == 0)
            {
                this.listBox1.Items.Add("No reviews for this movie yet.!!");
                return;
            }
            
            
            foreach(BusinessTier.Review review in reviews){

                string msg = string.Format("User {0}  :  Rating {1}",
                    review.UserID,
                    review.Rating);

                this.listBox1.Items.Add(msg);

            }
            
            
        }

        //
        // Average Rating:
        //
        private void cmdAvgRating_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            
            //
            // Compute average rating:
            //
            string name = txtRatingsMovieName.Text;
            name = name.Replace("'", "''");  // escape any single ' in the string:

            BusinessTier.Movie movie = BT.GetMovie(name);

            if (movie == null)
            {
                this.listBox1.Items.Add("Movie not found!!");
                return;
            }
            else
            {
                BusinessTier.MovieDetail details = BT.GetMovieDetail(movie.MovieID);
                listBox1.Items.Add("Average rating: " + details.AvgRating);
            }
            
        }

        //
        // Each Rating:
        //
        private void cmdEachRating_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            string name = txtRatingsMovieName.Text;
            name = name.Replace("'", "''");  // escape any single ' in the string:

            BusinessTier.Movie movie = BT.GetMovie(name);

            if (movie == null)
            {
                this.listBox1.Items.Add("Movie not found!!");
                return;
            }
            else
            {

                //Get individual ratings
                int rate1 = 0, rate2 = 0, rate3 = 0, rate4 = 0, rate5 = 0;

                //Get the reviews from the movie details function
                BusinessTier.MovieDetail details = BT.GetMovieDetail(movie.MovieID);
                IReadOnlyList<BusinessTier.Review> reviews = details.Reviews;

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

                listBox1.Items.Add("5: " + rate5);
                listBox1.Items.Add("4: " + rate4);
                listBox1.Items.Add("3: " + rate3);
                listBox1.Items.Add("2: " + rate2);
                listBox1.Items.Add("1: " + rate1);
                listBox1.Items.Add("Total: " + details.NumReviews);
            }
            

   
        }

        //
        // Add movie:
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

            if (checkMovie != null)
            {
                this.listBox1.Items.Add(String.Format("{0} already exists.", checkMovie.MovieName));
                return;
            }

            BusinessTier.Movie movie = BT.AddMovie(text);

            if (movie == null)
            {
                this.listBox1.Items.Add("Insert Failed?!!!");    
            }
            else
            {
                this.listBox1.Items.Add("Success, movie added.");
                this.listBox1.Items.Add("Movie ID: " + movie.MovieID.ToString());
            }
        }

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

            
            string name = txtInsertMovieName.Text;
            name = name.Replace("'", "''");  // escape any single ' in the string:

            BusinessTier.Movie movie = BT.GetMovie(name);

            if (movie == null)
            {
                this.listBox1.Items.Add("Movie not found.");    
            }
            else
            {
               
                int rating = Convert.ToInt32(lblRating.Text);
                int movieid = movie.MovieID;
                int userid = RandomNumberGenerator.Next(100000, 999999);  // 6-digit user ids:

                //Call our AddReviews and display results
                BusinessTier.Review review = BT.AddReview(movieid, userid, rating);
                if (review == null)
                {
                    this.listBox1.Items.Add("Review not added.");
                }
                else
                {
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

            IReadOnlyList<BusinessTier.Movie> movies = BT.GetTopMoviesByAvgRating(N);

            
            foreach(BusinessTier.Movie movie in movies)
            {
                //Loop and call movie details to get average
                BusinessTier.MovieDetail detail = BT.GetMovieDetail(movie.MovieID);

                string msg = string.Format("{0}: AVG={1}",
                    movie.MovieName,
                    detail.AvgRating);

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

            IReadOnlyList<BusinessTier.Movie> movies = BT.GetTopMoviesByNumReviews(N);

            foreach (BusinessTier.Movie movie in movies)
            {
                //Call movie details to get number of reviews
                BusinessTier.MovieDetail movieDetail = BT.GetMovieDetail(movie.MovieID);

                string msg = string.Format("{0} : {1}",
                     movie.MovieName,
                     movieDetail.NumReviews);

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

            IReadOnlyList<BusinessTier.User> users = BT.GetTopUsersByNumReviews(N);

            foreach (BusinessTier.User user in users)
            {
                //Call user details to get user number of reviews
                BusinessTier.UserDetail userDetail = BT.GetUserDetail(user.UserID);

                string msg = string.Format("{0} : {1}",
                     user.UserID,
                     userDetail.NumReviews);

                this.listBox1.Items.Add(msg);

            }

           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            IReadOnlyList<BusinessTier.Movie> movies = BT.GetMovies();

            if (movies.Count == 0)
            {
                this.listBox1.Items.Add("No Movies.");
            }
            else
            {
                foreach (BusinessTier.Movie movie in movies)
                {
                    this.listBox1.Items.Add(movie.MovieName);
                }
            }

        }

    }//class
}//namespace
