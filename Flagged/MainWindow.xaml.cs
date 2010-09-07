using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Yahoo.OAuth;
using System.Net;
using System.IO;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Threading;
using System.Globalization;

namespace Flagged
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<string, string> d;
        public int score = 0;
        SpeechSynthesizer synth = new SpeechSynthesizer();
        GrammarBuilder capitalsGrammer = new GrammarBuilder();
        Grammar quizGrammar;
        GrammarBuilder gbCapitals;
        SpeechRecognizer recog = new SpeechRecognizer();
        bool answered = false;
        int counter = 0;
        string currentAnswer = string.Empty;
        string consumerKey = Flagged.Properties.Resources.KEY; 
        string consumerSecret = Flagged.Properties.Resources.SECRET;    
        SpeechRecognitionEngine RecognitionEngine = new SpeechRecognitionEngine(new CultureInfo("en-US"));
           
           
           
        
        public MainWindow()
        {
            InitializeComponent();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
            synth.SelectVoice("Microsoft Anna");
            PrepareQuizData();
            RecognitionEngine.SetInputToDefaultAudioDevice();
            btnAnswer.IsEnabled = false;
            //RecognitionEngine.
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            answered = false;
            if (txtAnswer.Text == "")
            {

                RecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(quizGrammar_SpeechRecognized);
                //while (true)
                //{
                //synth.Speak("Hello America");
                RecognitionResult Result = RecognitionEngine.Recognize();
                StringBuilder Output = new StringBuilder();
                if (Result != null)
                {
                    foreach (RecognizedWordUnit Word in Result.Words)
                    {
                        Output.Append(Word.Text);
                    }
                }
            }
            else
            {
                bool result = (txtAnswer.Text.ToLower().Trim() == currentAnswer.ToLower().Trim());

                if (result)
                {
                    score = score + 5;
                    lblScore.Content = score;
                    synth.Speak("Absolutely Correct!");
                    lblResult.Content = "Correct!";
                }
                else
                {
                    synth.Speak("The answer is wrong!");
                    lblResult.Content = "Wrong!";
                }
                btnTrivia.IsEnabled = true;
                btnNext.IsEnabled = true;
                btnAnswer.IsEnabled = false;
                answered = true;
            }
        }


        void quizGrammar_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string response = e.Result.Text;

            //detach the event handler
            quizGrammar.SpeechRecognized -=
                new EventHandler<SpeechRecognizedEventArgs>(
                    quizGrammar_SpeechRecognized);

            bool result = (response.ToLower().Trim() == currentAnswer.ToLower().Trim());

            if (result)
            {
                score = score + 5;
                lblScore.Content = score;
                synth.Speak("Absolutely Correct!");
                lblResult.Content = "Correct!";
            }
            else
            {
                synth.Speak("The answer is wrong!");
                lblResult.Content = "Wrong!";
            }
            btnTrivia.IsEnabled = true;
            btnNext.IsEnabled = true;
            answered = true;

            

        }

        public void StartQuiz()
        {
            score = 0;
            answered = true;
            string country = d.Keys.ElementAt(0);

            btnAnswer.IsEnabled = false;
            btnNext.IsEnabled = false;
            currentAnswer = d[country];
                
            AskQuestion(country);

        }

        public void AskQuestion(string country)
        {

            string yql = "select thumbnail_url from search.images where query=\"" + country + " flag\" and mimetype like \"%jpeg%\" limit 1";
            lblResult.Content = "";
            var xml = QueryYahoo(yql, consumerKey, consumerSecret);
            string url = ParseXML(xml,"/query/results");
            //Image from URL
            imFlag.Source = new BitmapImage(new Uri(url, UriKind.Absolute));

            lblCountry.Content = country;            
            synth.Speak("Tell the capital of "+country);

            //Activate answer button
            btnAnswer.IsEnabled = true;
            btnNext.IsEnabled = false;
            btnTrivia.IsEnabled = false;
            counter++;
        }


        private static XmlDocument QueryYahoo(string yql, string consumerKey, string consumerSecret)
        {
            string url = "http://query.yahooapis.com/v1/yql?format=xml&diagnostics=false&q=" + Uri.EscapeUriString(yql);
            url = OAuth.GetUrl(url, consumerKey, consumerSecret);

            var req = System.Net.HttpWebRequest.Create(url);
            var xml = new XmlDocument();
            using (var res = req.GetResponse().GetResponseStream())
            {
                xml.Load(res);
            }
            return xml;
        }

        private string ParseXML(XmlDocument doc,string path)
        {
            string q = (from c in doc.DocumentElement.ChildNodes.Cast<XmlNode>() select c).Single().InnerText;  
            return q;
        }           

        private void PrepareQuizData()
        {
            d = new Dictionary<string, string>();
            d.Add("India", "New Delhi");
            d.Add("England", "London");
            d.Add("France", "Paris");
            d.Add("Pakistan", "Islamabad");
            d.Add("Spain", "Madrid");
            d.Add("Canada", "Toronto");
            d.Add("Germany", "Berlin");
            d.Add("Greece", "Athens");
            d.Add("Egypt", "Cairo");
            d.Add("Fiji", "Suva");
            d.Add("Italy", "Rome");
            d.Add("Japan", "Tokyo");
            d.Add("Zambia", "Lusaka");

            Choices chCapital = new Choices("New Delhi", "London", "Paris", "Islamabad", "Madrid", "Toronto", "Berlin", "Athens", "Cairo", "Suva", "Rome", "Tokyo", "Lusaka");
            gbCapitals = new GrammarBuilder();
            gbCapitals.Append(chCapital);
            quizGrammar = new Grammar(gbCapitals);
            RecognitionEngine.LoadGrammar(quizGrammar);
            //load the grammar into the recognizer
            //recog.LoadGrammar(quizGrammar);

        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (d.Count == counter)
            {
                //tell final score and end the dialog
                synth.Speak("You scored " + score + "points out of total " + (d.Count*5).ToString() + "points");
                MessageBox.Show("You scored " + score + "points out of total " + (d.Count * 5).ToString() + "points");

                this.Close();

            }
            else
            {
                btnAnswer.IsEnabled = false;
                string country = d.Keys.ElementAt(counter);
                currentAnswer = d[country];

                AskQuestion(country);
            }
        }

        private string ParseNewsXML(XmlDocument doc,string path)
        {
           // Set up namespace manager for XPath  
           //XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
           List<string> newsItem = new List<string>();
           //ns.AddNamespace("yahoo", "http://www.yahooapis.com/v1/base.rng");

           // Get forecast with XPath  
          // XmlNodeList nodes = doc.SelectNodes(path, ns);
            
          // return (nodes[0].InnerText==null)?String.Empty:(nodes[0].InnerText);
          // List<string> newsItem = new List<string>();
           string q = (from c in doc.DocumentElement.ChildNodes.Cast<XmlNode>() select c).Single().FirstChild.InnerText;
           if (q != null)
           {
               return q;
           }
           //var q = from c in doc.Descendants("site")
           //select (string)c.Element("name") + " -- " +(string)c.Element("url");
           return "no news for this location at this moment";
        }

        private void btnTrivia_Click(object sender, RoutedEventArgs e)
        {
            string yql = "select title,abstract from search.news where query=\"" + currentAnswer + "\" limit 1";

            var xml = QueryYahoo(yql, consumerKey, consumerSecret);
            string response = ParseNewsXML(xml, "/query/results");

            synth.Speak("Latest news for "+currentAnswer+ "is " + response);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartQuiz();
        }
    }
}


