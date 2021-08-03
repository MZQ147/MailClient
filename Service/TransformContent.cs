using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;



//�Խ����������ʼ����ݽ��н���
namespace MailClient.TransformContent
{
    public class Transform
    {
        public string Content { get; set; }
        public string[] MailTo { get; set; }
        public string MailFrom { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }
        public string[] ContentType { get; set; }
        //һ����text,multipart������,����ֵΪ��{"text","html","gbk"}
        public string TransferEncoding { get; set; }

        public Transform() { }

        //ԭ���ĺ���ֻ��������һ���ʼ����н���
        public void transform(string oriContent)
        {
            StreamReader stream;
            stream = new StreamReader(GenerateStreamFromString(oriContent));
            string row;
            while (stream.EndOfStream == false)
            {
                row = stream.ReadLine();

                if (row.IndexOf("From:") != -1&& row.IndexOf("<")!=-1)
                {
                    try
                    {
                        int i = row.IndexOf("<");
                        int j = row.IndexOf(">");
                        MailFrom = row.Substring(i + 1, j - i - 1);
                    }
                    catch (Exception e) { throw e.InnerException; }
                    continue;
                }
                if (row.IndexOf("To:") != -1 && row.IndexOf("<") != -1)
                {
                    try
                    {
                        
                        string[] AMailTo;
                        AMailTo = row.Substring(row.IndexOf(":") + 2).Trim().Split(' ');
                        MailTo = new string[3];
                        for(int k = 1; k < AMailTo.Length; k++)
                        {
                            MailTo[k - 1] = AMailTo[k];
                            if (MailTo[k - 1].IndexOf("<") != -1)
                            {
                                int i = MailTo[k-1].IndexOf("<");
                                int j = MailTo[k-1].IndexOf(">");
                                MailTo[k - 1] = MailTo[k - 1].Substring(i + 1, j - i - 1);
                            }
                        }


                    }
                    catch (Exception e) { throw e.InnerException; }
                    continue;
                }
                if (row.IndexOf("Subject:") != -1)
                {
                    try
                    {
                        Subject = SubTransform(row.Substring(row.IndexOf(":") + 2));

                    }
                    catch (Exception e) { throw e.InnerException; }
                    continue;
                }
                if (row.IndexOf("Date:") != -1)
                {
                    try
                    {
                        Date = row.Substring((row.IndexOf(":") + 2));
                    }
                    catch (Exception e) { throw e.InnerException; }
                    continue;
                }
                if (row.IndexOf("This is a multi-part message in MIME format") != -1)
                {
                    while (stream.EndOfStream == false)
                    {
                        row = stream.ReadLine();
                        if (row.IndexOf("Content-Type") != -1)
                        {
                            string[] str, rt, s3;
                            string s = row;
                            rt = new string[3];
                            s += stream.ReadLine(); //����һ�е�charset����
                            s = s.Replace('"', ' ');
                            str = s.Split(';');
                            str[0] = str[0].Trim();//�ֱ�������
                            str[1] = str[1].Trim();
                            s3 = str[0].Substring(str[0].IndexOf(" ") + 1).Trim().Split('/');
                            rt[0] = s3[0];
                            rt[1] = s3[1];
                            rt[2] = str[1].Substring(str[1].IndexOf("=") + 1).Trim();
                            try
                            {
                                this.ContentType = rt;
                            }
                            catch (Exception e) { throw e.InnerException; }
                            continue;
                        }
                        if (row.IndexOf("Content-Transfer-Encoding:") != -1)
                        {
                            try
                            {
                                string ecoding= row.Substring((row.IndexOf(":") + 2));                                                               
                                this.TransferEncoding = ecoding;                               
                                //��content���ݷ���Content-Transfer-Ecodong����
                                row = stream.ReadLine();
                                
                                if(row == "") { row = stream.ReadLine(); }
                                if (row.IndexOf("==") != -1)
                                {
                                    
                                }
                                else
                                {
                                    while (row.IndexOf("------=_NextPart") == -1)
                                    {
                                        if (ContentType[1] == "html")
                                        {
                                            Content += row;
                                            row = stream.ReadLine();
                                        }
                                        else { row = stream.ReadLine(); }

                                    }
                                }/*
                                if (row == "") { row = stream.ReadLine(); }
                                while(row != "")
                                {
                                    Content += row;
                                    row = stream.ReadLine();
                                }*/
                            }
                            catch (Exception e) { throw e.InnerException; }
                            continue;
                        }

                    }
                }
            }
            //Console.WriteLine(ContentType[0] + ContentType[1] + ContentType[2]);
            //�����ʼ������ݽ��н���.���ò�ͬ����ʽ
            if (ContentType[0] == "text")
            {
                switch (TransferEncoding)
                {
                    case "base64":
                        Content = Encoding.GetEncoding(ContentType[2].ToUpper()).GetString(Convert.FromBase64String(Content));
                        break;
                    case "quoted-printable":
                        Content = QuotedPrintable(GetContent(oriContent), ContentType[2]);
                        break;
                    default:
                        Content = GetContent(oriContent).Trim();
                        break;
                        ;
                }
            }
            //���ܲ�ͬ��������ʽ����Կؼ���ѡ�����Ӱ�죬�����Ȳ�������������˵
            //�������ڸ��������ӣ������û�д���
            /*
            if (ContentType[1] == "html")
            {
                //�л���html���͵�������ؼ�
                webBrower.DucumentText = Content;
            }
            else { richTextBoxContent.Text = Content; } //�ı��ļ�
            */
        }


        public static string DecodeBase64(string code_type, string code)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        }

        //�����ַ�������һ���ļ���
        private Stream GenerateStreamFromString(string s)
        {

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;

        }


        //ת�������ʽ
        private string SubTransform(string subject)
        {
            subject = subject.Trim();
            //�����������ʽ
            Regex regex = new Regex(@"=\?([\s\S]+)\?=");
            MatchCollection match = regex.Matches(subject);
            if (match.Count <= 0) { }
            else
            {
                for (int i = 0; i < match.Count; i++)
                {
                    subject = subject.Replace(match[i].Value, ConvertStr(match[i].Value));
                }
            }
            return subject;
        }

        private string ConvertStr(string s)
        {
            string[] divString = s.Split('?');
            if (divString[2].ToUpper() == "B")
            {
                s = DecodeBase64("GBK", divString[3]);

            }//����quoted-printableת���ַ�
            else { s = QuotedPrintable(divString[3], divString[1]); }
            return s;
        }

        private string QuotedPrintable(string s, string codeType)
        {
            s = s.Replace("=\r\n", "");
            s = s.Replace("=\n", "");
            int length = s.Length;
            string dest = string.Empty;
            int i = 0;
            while (i < length)
            {
                string temp = s.Substring(i, 1);
                if (temp == "=")
                {
                    try
                    {
                        int code = Convert.ToInt32(s.Substring(i + 1, 2), 16);
                        if (Convert.ToInt32(code.ToString(), 10) < 127)
                        {
                            dest += ((char)code).ToString();

                            i = i + 3;
                        }
                        else
                        {
                            try
                            {
                                dest += Encoding.GetEncoding(codeType.ToUpper()).GetString(new byte[] {
                                Convert.ToByte(s.Substring(i + 1, 2), 16),

                                Convert.ToByte(s.Substring(i + 4, 2), 16) });

                            }
                            catch (Exception e) { throw e.InnerException; }
                            i += 6;
                        }
                    }
                    catch { i++; continue; }
                }
                else { dest += temp; i++; }
            }
            return dest;
        }

        //��ȡ�ʼ����ı����ݣ�wδ���룩
        private string GetContent(string oriContent)
        {
            string s = "";
            StreamReader stream;
            stream = new StreamReader(GenerateStreamFromString(oriContent));
            string row = stream.ReadLine();
            while (row != "") { row = stream.ReadLine(); }
            //��ʱ�ڵ�һ��CRLF��λ�� ����ȡֱ��ȫ����ȡ�ķ���������Ҫ�Ľ�
            while (stream.EndOfStream == false)
            {
                row = stream.ReadLine();
                s += row;
            }
            return s;
        }

    }
}