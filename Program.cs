using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MlinetlesFile
{
    class Program
    {
        static void Main(string[] args)
        {
            string inPath = Path.Combine("Files"); // 要打包的文件所在的目录路径

            string outPath = Path.Combine("MlineFiles", "Mlinetles.ple"); // 打包后的文件路径

            Console.WriteLine("输入1打包Files目录下的所有文件至MlineFiles,输入2解包MlineFiles下的所有已打包文件至Output");

            int input;

            while(true)
            {
                try
                {
                    input = Convert.ToInt32(Console.ReadLine());

                    if(input == 1 || input == 2) break;

                    else throw new Exception();
                }
                catch(Exception)
                {
                    Console.WriteLine("byd不好好输是吧");

                    continue;
                }
            }

            switch(input)
            {
                case 1:
                    Writer(inPath, outPath);

                    break;

                case 2:
                    Reader(outPath);

                    break;
            }
        }

        public static void Writer(string directoryPath, string outputPath)
        {
            // 获取目录中的所有文件名
            string[] fileNames = Directory.GetFiles(directoryPath);

            Directory.CreateDirectory("MlineFiles");

            // 创建二进制写入器，用于写入打包后的文件
            using (BinaryWriter writer = new(File.Open(outputPath, FileMode.Create)))
            {
                // 首先写入一个整型变量，表示有多少个子文件
                writer.Write(fileNames.Length);

                StringBuilder head = new();

                // 逐个写入每个文件名
                foreach (string fileName in fileNames)
                {
                    FileInfo file = new(fileName);
                    head.Append(Path.GetFileName(fileName) + '|' + file.Length + '|');
                }

                string files = head.ToString();

                writer.Write(files.Length);

                writer.Write(files);

                // 逐个将每个文件的内容按顺序写入进去
                foreach (string fileName in fileNames)
                {
                    byte[] fileContents = File.ReadAllBytes(fileName);
                    writer.Write(fileContents);
                }
            }
        }

        public static void Reader(string filepath)
        {
            FileInfo fileInfo = new(filepath);

            (int fileNum, int Headed) Head = new();

            byte[] returning;

            using(BinaryReader reader = new(File.Open(filepath, FileMode.Open)))
            {
                Head = (reader.ReadInt32(), reader.ReadInt32());

                returning = reader.ReadBytes(((int)fileInfo.Length));
            }

            Directory.CreateDirectory("Output");

            var fileContent = (info : Encoding.UTF8.GetString(returning[1..Head.Headed]), text : Encoding.UTF8.GetString(returning[(Head.Headed + 1)..]));

            int haveWritten = 0;

            var fileOperated = fileContent.info.Split('|').Where(item => item != null);

            var fileOperatedAlpha = fileOperated.Where((item, index) => index % 2 == 0).ToList();

            var fileOperatedBeta = fileOperated.Where((item, index) => index % 2 != 0).ToList();

            List<(string, int)> files = new();

            foreach(var FileItem in fileOperatedAlpha)
            {
                files.Add((FileItem, Convert.ToInt32(fileOperatedBeta[fileOperatedAlpha.IndexOf(FileItem)])));
            }

            foreach((string, int) fileItem in files)
            {
                using(BinaryWriter writer = new(File.Open(Path.Combine("Output", fileItem.Item1), FileMode.Create), Encoding.UTF8))
                {
                    writer.Write(fileContent.text[haveWritten..(haveWritten + Convert.ToInt32(fileItem.Item2))].ToString());
                }

                Console.WriteLine(fileContent.text[haveWritten..(haveWritten + Convert.ToInt32(fileItem.Item2))].ToString());

                haveWritten += Convert.ToInt32(fileItem.Item2);
            }

            Console.WriteLine(Head.fileNum);
        }
    }
}