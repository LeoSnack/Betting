using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using Akumu.Antigate;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace RegVDS
{
    public partial class Form1 : Form
    {
        string myDoc = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string url;
        List<string> dataContent = new List<string>();
        List<IWebElement> TotalOfPage = new List<IWebElement>();
        List<IWebElement> voiceTotalOfPage = new List<IWebElement>();
        List<string> linkToGame = new List<string>();
        List<IWebElement> linkToResultPage = new List<IWebElement>();
        string countryGameFile;
        string nameGameFile;
        private ChromeDriver Browser;
        string namePage;
        string content;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            countryGameFile = "China";
            nameGameFile = "CBA";
            dataContent.Clear();
            url = "https://www.oddsportal.com/basketball/china/cba/results/#/page/7/";
            GetData();
        }     

        private void browserInit()
        {
            ChromeOptions option = new ChromeOptions();
            option.AcceptInsecureCertificates = true;
            option.AddArgument("--disable-notifications");
            option.AddArgument("--disable-infobars");
            option.AddArgument("--ignore-certificate-errors");
            option.AddArgument("--ignore-ssl-errors");
            option.AddArgument("--allow-running-insecure-content");
            option.AddArgument("--allow-silent-push");
            option.AddArgument("--disable-prompt-on-repost");
            option.AddArgument("--enable-fast-unload");
            option.AddArgument("--fast");
            option.AddArgument("--mute-audio");
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            Browser = new ChromeDriver(service, option);
            Browser.Manage().Window.Maximize();
        }

        public void GetData()
        {
            linkToResultPage.Clear();
            browserInit();
            Browser.Navigate().GoToUrl(url);
            Thread.Sleep(5000);

            IWebElement NameOfPage = Browser.FindElement(By.XPath("//div[@id='col-content']/h1"));
            linkToResultPage = Browser.FindElements(By.XPath("//td[@class='name table-participant']/a")).ToList();
            namePage = NameOfPage.Text;
            namePage = namePage.Replace(@"/", "");
            ResultFileHeadMake(namePage);

            Thread.Sleep(10000);
            foreach(IWebElement hrefEl in linkToResultPage)
            {
                string href = hrefEl.GetAttribute("href");
                string lastHref = "#over-under;1";
                string allHref = href + lastHref;
                linkToGame.Add(allHref);
            }

            for (int i = 0; i < linkToResultPage.Count; i++)
            {
                GamePage(linkToGame[i]);
            }
            Browser.Quit();
        }
        
        public void GamePage(string uri)
        {
            string NameGame = "";
            string resultGame = "";
            string totalPage = "";
            string massTotal = "";
            TotalOfPage.Clear();
            voiceTotalOfPage.Clear();
            dataContent.Clear();
            int maxInd = 0;
            string to = "";

            Browser.Navigate().GoToUrl(uri);
            Thread.Sleep(5000);
            IWebElement NameOfGame = Browser.FindElement(By.XPath("//div[@id='col-content']/h1"));
            NameGame = NameOfGame.Text; //Name

            IWebElement ResultOfPage = Browser.FindElement(By.XPath("//p[@class='result']"));
            resultGame = ResultOfPage.Text;
            int chInd = resultGame.IndexOf('(');
            resultGame = resultGame.Remove(0, chInd); //Result

            voiceTotalOfPage = Browser.FindElements(By.XPath("//div[@class='table-container']//span[@class='odds-cnt']")).ToList();
            TotalOfPage = Browser.FindElements(By.XPath("//div[@class='table-container']//strong/a")).ToList();

            voiceTotalOfPage = voiceTotalOfPage.Where(x => x.Text != String.Empty).ToList();
            TotalOfPage = TotalOfPage.Where(x => x.Text != String.Empty).ToList();


            for (int i = 0; i < voiceTotalOfPage.Count; i++)
            {
                massTotal = massTotal + voiceTotalOfPage[i].Text;
            }

            to = ProcessStr(massTotal);
            maxInd = GetMaxIndexTotal(to);
            totalPage = TotalOfPage[maxInd].Text; //TOTAL

            dataContent.Add(NameGame);
            dataContent.Add(resultGame);
            dataContent.Add(totalPage);

            ResultFileDataWrite(namePage, dataContent);
            int k = 0;
        }

        public void DeleteEmpty(List<IWebElement> List)
        {
            for (int j = 0; j < 10; j++)
            {
                if (List[0].Text == "")
                {
                    List.RemoveAt(0);
                }
            }
        }

        public string ProcessStr(string st)
        {
            for(int i = 0; i < st.Count(); i++)
            {
                if(st[i] == '(')
                {
                    st = st.Replace("(", "");
                }
                else if(st[i] == ')')
                {
                    st = st.Replace(")", ",");
                }
            }
            int ind = st.Length - 1;
            st = st.Remove(ind);
            return st;
        }

        public int GetMaxIndexTotal(string s)
        {
            int[] tot = s.Split(',').Select(snum => int.Parse(snum)).ToArray();
            int max = tot.Max();
            int index = Array.FindLastIndex(tot, delegate (int i) { return i == max; });
            return index;
        }

        public void ResultFileHeadMake(string name)
        {
            string newFileName = @"C:\Users\user\Desktop\betting\" + countryGameFile + @"\" + nameGameFile + @"\" + name + ".csv";

            if (!File.Exists(newFileName))
            {
                string clientHeader = "Name" + ";" + "Result" + ";" + "OverUnder" + Environment.NewLine;

                File.WriteAllText(newFileName, clientHeader);
            }
        }

        
        public void ResultFileDataWrite(string name, List<string> dataContent)
        {
            string newFileName = @"C:\Users\user\Desktop\betting\" + countryGameFile + @"\" + nameGameFile + @"\" + name + ".csv";
            content = dataContent[0] + ";" + dataContent[1] + ";" + dataContent[2] + Environment.NewLine;
            File.AppendAllText(newFileName, content);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }
    }
}
