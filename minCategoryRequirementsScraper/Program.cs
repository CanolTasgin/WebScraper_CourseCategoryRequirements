using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;


/* Workflow:
 * 1) One by one iterates through all majors 2010 to 2019 (+Fall and Spring Semesters) from Sabanci Website
 * 2) Make arrangements according to their specific pages so program can take the necessary information (There are differences between html pages)
 * 3) Takes first and last indexes of desired texts according to html pattern in the webpage
 * 4) Writes desired texts to the file by taking information between last and first indexes
 * 
 * There are at least 3 pages related with specific major. For example there is a main page for BSCS 2015 Fall Entry(Computer Science). And 3 other pages with Core, Area and Free Courses in them.
 * 
 * 5) Program one by one checks all webpages and outputs all information (Course code, Course Name and SU Credit) to the file. Main page is analized by all headers included in it.
*/

namespace minCategoryRequirementsScraper
{
    class MainClass
    {
        public static void Main(string[] args) //Iterates trough all majors and calls another function that calls scraper
        {
            /////
            int faculty = 0;

            for (int i = 1; i <= 3; i++)
            {
                faculty = i;

                string majorcode = "";

                
                if (faculty == 1)
                {
                    for (int j = 1; j <= 6; j++)
                    {
                        if (j == 1)
                        {
                            majorcode = "BSCS";  //8
                            ScrapperCaller(majorcode);
                        }

                        else if (j == 2)
                        {
                            majorcode = "BSEE";  //8
                            ScrapperCaller(majorcode);
                        }
                        else if (j == 3)
                        {
                            majorcode = "BSMS";  //8
                            ScrapperCaller(majorcode);
                        }
                        else if (j == 4)
                        {
                            majorcode = "BSMAT"; //6
                            ScrapperCaller(majorcode);
                        }
                        else if (j == 5)
                        {
                            majorcode = "BSME";  //8
                            ScrapperCaller(majorcode);
                        }
                        else if (j == 6)
                        {
                            majorcode = "BSBIO"; //6
                            ScrapperCaller(majorcode);
                        }
                    }
                }
                else if (faculty == 2)
                {
                    for (int k = 1; k <= 4; k++)
                    {
                        if (k == 1)
                        {
                            majorcode = "BAECON"; //7
                            ScrapperCaller(majorcode);
                        }
                        else if (k == 2)
                        {
                            majorcode = "BASPS";  //6
                            ScrapperCaller(majorcode);
                        }

                        else if (k == 3)
                        {
                            majorcode = "BAPSY";  //7
                            ScrapperCaller(majorcode);
                        }

                        else if (k == 4)
                        {
                            majorcode = "BAVACD"; //7}
                            ScrapperCaller(majorcode);
                        }
                    }
                }
                else if (faculty == 3)
                {
                    majorcode = "BAMAN";      //6
                    ScrapperCaller(majorcode);
                }
            }
        }

        public static void ScrapperCaller(string majorcod) // Iterates through entry years and semesters and calls scraper
        {
            string yearSemester = "";

            for (int i = 2010; i <= 2019; i++)
            {
                for (int j = 1; j <= 2; j++)
                {
                    yearSemester = i.ToString() + "0" + j.ToString();
                    string url1 = "https://www.sabanciuniv.edu/tr/aday-ogrenciler/degree-detail?SU_DEGREE.p_degree_detail%3fP_TERM=" + yearSemester + "&P_PROGRAM=" + majorcod + "&P_SUBMIT=&P_LANG=TR&P_LEVEL=UG";
                   
                    Scraper(url1,yearSemester, majorcod);
                }
            }
        }

        public static void Scraper(string url, string entr, string mjcode)
        {
            if (Directory.Exists(@mjcode))
            {
                Console.WriteLine(mjcode + "directory already exists. Moving on");
            }
            else
                Directory.CreateDirectory(@mjcode);

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@mjcode + "/" + mjcode + "_" + entr + "_CatReq")) //Create a folder with current major code and create a file as 'majorCode_entryYear'
            {
                WebRequest request = WebRequest.Create(url);

                WebResponse response = request.GetResponse();

                Console.WriteLine(((HttpWebResponse)response).StatusDescription); //Confirm the connection

                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd(); // get the server response (html)

                    string firstCategories = "\"text-decoration:none\">";
                    string lastCategories = "</a></td>\n<td style=\"text-align:center\">- </td>\n<td style=\"text-align:center\">";
                    List<int> indexes = new List<int>();
                    for (int index = 0; ; index += firstCategories.Length)
                    {
                        index = responseFromServer.IndexOf(firstCategories, index);
                        if (index == -1)
                            break;
                        indexes.Add(index);
                    }

                    List<int> lastIndexes = new List<int>();
                    for (int index = 0; ; index += lastCategories.Length)
                    {
                        index = responseFromServer.IndexOf(lastCategories, index);
                        if (index == -1)
                            break;
                        lastIndexes.Add(index);
                    }

                    while (lastIndexes.Count() != indexes.Count())
                        indexes.RemoveAt(indexes.Count - 1);

                    for (int i = lastIndexes.Count() - 1; i > 0; i--)
                    {
                        string credit = responseFromServer.Substring(lastIndexes[i] + lastCategories.Length, 2);
                        if (credit.Contains("-"))
                        {
                            lastIndexes.RemoveAt(i);
                            indexes.RemoveAt(i);
                        }
                    }

                    List<string> categories = new List<string>();
                    for (int i = 0; i < indexes.Count(); i++)
                    {
                        categories.Add(responseFromServer.Substring(indexes[i] + firstCategories.Length, lastIndexes[i] - indexes[i] - firstCategories.Length));
                    }


                    List<string> categoryCredits = new List<string>();
                    for (int i = 0; i < lastIndexes.Count(); i++)
                    {
                        categoryCredits.Add(responseFromServer.Substring(lastIndexes[i] + lastCategories.Length, 2));
                        if (categoryCredits[categoryCredits.Count() - 1].Substring(categoryCredits[categoryCredits.Count() - 1].Length - 1) == " ")
                        {
                            categoryCredits[categoryCredits.Count() - 1] = categoryCredits[categoryCredits.Count() - 1].Substring(0, categoryCredits[categoryCredits.Count() - 1].Length - 1);
                        }
                    }

                    for (int i = 0; i < categories.Count(); i++)
                    {
                        file.WriteLine(categories[i]+"<->"+categoryCredits[i]);
                    }
                  
                }
               
            }
        }
    }
}