/*
 * UOAM,UOMAPPER MAP MARKER FIX FOR CLASSICUO
 * http://www.github.com/odhinn
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace MarkerFixer
{
    class Program
    {
        static string mapsPath = AppDomain.CurrentDomain.BaseDirectory;
        static string[] mapFiles = Directory.GetFiles(mapsPath, "*.map")
            .Union(Directory.GetFiles(mapsPath, "*.csv"))
            .Union(Directory.GetFiles(mapsPath, "*.xml")).ToArray();
        static bool IsFixed = false;
        static int totalCount, totalFileCount= 0;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                List<string> xPath = new List<string>();
                List<string> wrongFiles = new List<string>();

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower().Contains(".xml") || args[i].ToLower().Contains(".map") || args[i].ToLower().Contains(".csv"))
                    {
                        xPath.Add(args[i]);
                    }
                    else if(Directory.Exists(args[i]))
                    {
                        string[] directory = Directory.GetFiles(args[i]);
                        for (int a = 0; a < directory.Length; a++)
                        {
                            if (directory[a].ToLower().Contains(".xml") || directory[a].ToLower().Contains(".map") || directory[a].ToLower().Contains(".csv"))
                            {
                                xPath.Add(directory[a]);
                            }
                            else
                                wrongFiles.Add(directory[a]);
                        }
                    }
                    else
                        wrongFiles.Add(args[i]);   
                }
                
                if(wrongFiles.Count > 0)
                {
                    for (int i = 0; i < wrongFiles.Count; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[ ERROR: {0} has wrong format or extension. It cannot be fixed. ]", Path.GetFileName(wrongFiles[i]));
                        Console.ResetColor();
                    }
                }

                fixFacets(xPath.ToArray());
            }
            else
            {
                Console.WriteLine("Marker Fixer v.{0}", Assembly.GetExecutingAssembly().GetName().Version.Major.ToString());
                Console.WriteLine("\nPress ENTER to start.");

                do
                {
                    while (!Console.KeyAvailable)
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter & !IsFixed)
                        {

                            fixFacets(mapFiles);
                        }
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
        }

        static void fixFacets(string[] xpath)
        {
            IsFixed = true;

            foreach (string mapFile in xpath)
            {
                if (File.Exists(mapFile) &  new FileInfo(mapFile).IsReadOnly != true )
                {

                    #region UOMapper
                    if (mapFile != null && Path.GetExtension(mapFile).ToLower().Equals(".xml"))
                    {
                        Console.WriteLine("\nFixing >>> {0}", Path.GetFileName(mapFile));
                        XDocument doc = XDocument.Load(mapFile);
                        List<XElement> elementList = new List<XElement>();

                        int xmlCount = 0;
                        
                        foreach (XElement cell in doc.Element("Pack").Elements("Marker"))
                        {
                            //divide uoam facet 0(fel/tram) to classicuo facet 0(fel) and facet 1(tram)
                            if (Convert.ToInt32(cell.Attribute("Facet").Value) == 0)
                            {
                                #region Felucca
                                XElement tempFel = XElement.Parse(cell.ToString());
                                tempFel.Attribute("Facet").Value = (Convert.ToInt32(tempFel.Attribute("Facet").Value)).ToString();
                                elementList.Add(tempFel);
                                #endregion

                                #region Trammel
                                XElement tempTram = XElement.Parse(cell.ToString());
                                tempTram.Attribute("Facet").Value = (Convert.ToInt32(tempTram.Attribute("Facet").Value) + 1).ToString();
                                elementList.Add(tempTram);
                                #endregion
                                xmlCount++;
                                totalCount++;

                            }
                            else if (Convert.ToInt32(cell.Attribute("Facet").Value) >= 1)
                            {
                                XElement temp = XElement.Parse(cell.ToString());
                                temp.Attribute("Facet").Value = (Convert.ToInt32(temp.Attribute("Facet").Value) - 1).ToString();
                                elementList.Add(temp);
                                xmlCount++;
                                totalCount++;
                            }
                            
                        }
                        doc.Root.ReplaceNodes(elementList);
                        doc.Save(mapFile);
                        FileInfo block = new FileInfo(mapFile);
                        //set file readyonly so next run it won't try to fix it again.
                        block.IsReadOnly = true;
                        if (xmlCount == 0)
                        {
                            Console.WriteLine("\n[ {0} doesn't have incorrect markers. ]", Path.GetFileName(mapFile));
                        }
                        else
                            Console.WriteLine("\n[ Total {0} markers are fixed in {1} ]",xmlCount, Path.GetFileName(mapFile));
                        totalFileCount++;
                    }
                    #endregion
                    #region UOAM
                    else if (mapFile != null && Path.GetExtension(mapFile).ToLower().Equals(".map"))
                    {
                        Console.WriteLine("\nFixing >>> {0}", Path.GetFileName(mapFile));
                        StringBuilder newFile = new StringBuilder();
                        int mapCount = 0;

                        string[] file = File.ReadAllLines(mapFile);

                        foreach (string linez in file)
                        {
                            if (string.IsNullOrEmpty(linez) || linez.Equals("3"))
                            {
                                newFile.Append(linez + "\r\n");
                                continue;
                            }

                            if (linez.Substring(0, 1).Equals("+") || linez.Substring(0, 1).Equals("-"))
                            {

                                string sline = linez.Substring(linez.IndexOf(':') + 2);

                                string[] splits = sline.Split(' ');

                                if (splits.Length <= 1)
                                {
                                    newFile.Append(linez + "\r\n");
                                    continue;
                                }

                                int facetID = int.Parse(splits[2]);

                                //divide uoam facet 0(fel/tram) to classicuo facet 0(fel) and facet 1(tram)
                                if(facetID == 0)
                                {

                                    #region Felucca
                                    string tempFel = linez.Remove(linez.IndexOf(':') + 2);
                                    for (int i = 0; i < splits.Length; i++)
                                    {

                                        if (i == 2)
                                        {
                                            tempFel += (int.Parse(splits[i])).ToString() + " ";
                                        }
                                        else
                                        {
                                            tempFel += splits[i] + " ";
                                        }
                                    }
                                    newFile.Append(tempFel + "\r\n");
                                    #endregion
                                    #region Trammel
                                    string tempTram = linez.Remove(linez.IndexOf(':') + 2);

                                    for (int i = 0; i < splits.Length; i++)
                                    {

                                        if (i == 2)
                                        {
                                            tempTram += (int.Parse(splits[i] + 1)).ToString() + " ";

                                        }
                                        else
                                        {
                                            tempTram += splits[i] + " ";
                                        }
                                    }
                                    newFile.Append(tempTram + "\r\n");
                                    mapCount++;
                                    totalCount++;
                                    #endregion
                                    continue;


                                }
                                else if (facetID >=1 )
                                {
                                    string temp = linez.Remove(linez.IndexOf(':') + 2);
                                    for(int i = 0; i < splits.Length; i++)
                                    {
                                        
                                        if (i==2)
                                        {
                                            temp += (int.Parse(splits[i]) - 1).ToString()+ " ";
                                        }
                                        else
                                        { 
                                        temp += splits[i] + " ";
                                        }
                                    }
                                    newFile.Append(temp + "\r\n");
                                    mapCount++;
                                    totalCount++;
                                    continue;
                                }
                            }

                            newFile.Append(linez + "\r\n");
                        }

                        File.WriteAllText(mapFile, newFile.ToString());
                        FileInfo block = new FileInfo(mapFile);
                        //set file readyonly so next run it won't try to fix it again.
                        block.IsReadOnly = true;
                        if (mapCount == 0)
                        {
                            Console.WriteLine("\n[ {0} doesn't have incorrect markers. ]", Path.GetFileName(mapFile));
                        }
                        else
                            Console.WriteLine("\n[ Total {0} markers are fixed in {1} ]", mapCount, Path.GetFileName(mapFile));
                        totalFileCount++;
                    }
                    #endregion
                    /*IF YOU HAVE OLD CSV FILES WITH FACET 0(FEL/TRAM), FACET 1(FEL), FACET 2(TRAM) PUT IT IN DIRECTORY
                      OTHERWISE DON'T PUT IT IN PROGRAM DIRECTORY OR DRAG DROP INTO MARKERFIXER.EXE */
                    #region CSV (Custom Markers)
                    else if (mapFile != null && Path.GetExtension(mapFile).ToLower().Equals(".csv"))
                    {

                        Console.WriteLine("\nFixing >>> {0}", Path.GetFileName(mapFile));
                        StringBuilder newFile = new StringBuilder();
                        int csvCount = 0;

                        string[] file = File.ReadAllLines(mapFile);

                        foreach (string linez in file)
                        {
                            if (string.IsNullOrEmpty(linez))
                            {
                                newFile.Append(linez + "\r\n");
                                continue;
                            }

                            string[] splits = linez.Split(',');

                            if (splits.Length <= 1)
                            {
                                    newFile.Append(linez + "\r\n");
                                    continue;
                            }

                            int facetID = int.Parse(splits[2]);

                            //divide uoam facet 0(fel/tram) to classicuo facet 0(fel) and facet 1(tram)
                            if (facetID == 0)
                            {
                                #region Felucca
                                string tempFel = "";
                                for (int i = 0; i < splits.Length; i++)
                                {
                                    if (i == 2)
                                    {
                                        tempFel += (int.Parse(splits[i])).ToString() + ",";
                                    }
                                    else
                                    {
                                        if (i == splits.Length - 1)
                                            tempFel += splits[i];
                                        else
                                            tempFel += splits[i] + ",";
                                    }
                                }
                                newFile.Append(tempFel + "\r\n");
                                #endregion
                                #region Trammel
                                string tempTram = "";
                                for (int i = 0; i < splits.Length; i++)
                                {
                                    if (i == 2)
                                    {
                                        tempTram += (int.Parse(splits[i]) + 1).ToString() + ",";
                                    }
                                    else
                                    {
                                        if (i == splits.Length - 1)
                                            tempTram += splits[i];
                                        else
                                            tempTram += splits[i] + ",";
                                    }
                                }
                                newFile.Append(tempTram + "\r\n");
                                csvCount++;
                                totalCount++;
                                #endregion
                                continue;
                            }
                            else if(facetID >= 1)
                            {
                                string temp = "";
                                for (int i = 0; i < splits.Length; i++)
                                {
                                    if (i == 2)
                                    {
                                        temp += (int.Parse(splits[i]) - 1).ToString() + ",";
                                    }
                                    else
                                    {
                                        if (i == splits.Length - 1)
                                            temp += splits[i];
                                        else
                                            temp += splits[i] + ",";
                                    }
                                }
                                newFile.Append(temp + "\r\n");
                                csvCount++;
                                totalCount++;
                                continue;

                            }
                            newFile.Append(linez + "\r\n");

                        }

                        File.WriteAllText(mapFile, newFile.ToString());
                        FileInfo block = new FileInfo(mapFile);
                        //set file readyonly so next run it won't try to fix it again.
                        block.IsReadOnly = true;
                        if(csvCount == 0)
                        {
                            Console.WriteLine("\n[ {0} doesn't have incorrect markers. ]", Path.GetFileName(mapFile));
                        }
                        else
                            Console.WriteLine("\n[ Total {0} markers are fixed in {1} ]", csvCount, Path.GetFileName(mapFile));
                        totalFileCount++;
                    }
                    #endregion
                }
            }
            if (totalCount > 0)
            {
                Console.WriteLine("\n[ Result: {0} facets are fixed in {1} {2}. ]", totalCount.ToString(), totalFileCount.ToString(), totalFileCount > 1 ? "files":"file");
            }
            else
                Console.WriteLine("\nThere is nothing left.");

            Console.WriteLine("\nPress ESC to exit.");

            while (!Console.KeyAvailable)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    Environment.Exit(1);
            }
        }

    }
}
