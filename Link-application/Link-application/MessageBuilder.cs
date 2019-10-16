using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Link_application
{
    class MessageBuilder
    {
        // Fields
        private string messageBeginMarker;
        private string messageEndMarker;
        private string bufferedData;

        // Constructor
        public MessageBuilder(String messageBeginMarker, String messageEndMarker)
        {
            this.messageBeginMarker = messageBeginMarker;
            this.messageEndMarker = messageEndMarker;

            bufferedData = "";
        }

        // Methodes
        public void Append(String data)
        {
            if (data != null)
            {
                bufferedData += data;
            }
        }

        public void ProcessBuffer(string focusMarker, string meditatieMarker, string winkMarker, string sendMarker)
        {
            while (bufferedData != "")
            {
                int beginIndex = bufferedData.IndexOf(messageBeginMarker);
                if (beginIndex != -1)
                {
                    int endIndex = bufferedData.IndexOf(messageEndMarker, beginIndex);
                    if (endIndex != -1)
                    {
                        if (bufferedData.IndexOf(focusMarker) == 1)
                        {
                            string foundMessage = bufferedData.Substring(beginIndex + 2, (endIndex - 2));
                            int waarde = Convert.ToInt32(foundMessage);

                            Program.linkform.Focus = waarde;
                            Program.linkform.lbxFocus.Items.Insert(0, waarde);

                            if (waarde > 0)
                            {
                                Program.linkform.focustotaal += waarde;
                                Program.linkform.focusaantal += 1;
                            }

                            bufferedData = bufferedData.Substring(endIndex + 1);
                        }
                        else if (bufferedData.IndexOf(meditatieMarker) == 1)
                        {
                            string foundMessage = bufferedData.Substring(beginIndex + 2, (endIndex - 2));
                            int waarde = Convert.ToInt32(foundMessage);
                            
                            Program.linkform.Meditatie = waarde;
                            Program.linkform.lbxMeditatie.Items.Insert(0, waarde);

                            if (waarde > 0)
                            {
                                Program.linkform.meditatietotaal += waarde;
                                Program.linkform.meditatieaantal += 1;
                            }

                            bufferedData = bufferedData.Substring(endIndex + 1);
                        }
                        else if (bufferedData.IndexOf(winkMarker) == 1)
                        {
                            Program.linkform.Winker();
                            bufferedData = bufferedData.Substring(endIndex + 1);
                        }
                        else if (bufferedData.IndexOf(sendMarker) == 1)
                        {
                            bufferedData = bufferedData.Substring(endIndex + 1);
                        }
                        else
                        {
                            // Wanneer de marker niet herkend wordt de message verwijderen
                            bufferedData = bufferedData.Substring(endIndex + 1);
                        }
                    }
                    else
                    {
                        Clear();
                    }
                }
                else
                {
                    Clear();
                }
            }
        }

        public bool CheckLink(string linkMarker, string sendMarker)
        {
            bool result = false;
            while (bufferedData != "")
            {
                int beginIndex = bufferedData.IndexOf(messageBeginMarker);
                if (beginIndex != -1)
                {
                    int endIndex = bufferedData.IndexOf(messageEndMarker, beginIndex);
                    if (endIndex != -1)
                    {
                        if (bufferedData.IndexOf(sendMarker) == 1)
                        {
                            bufferedData = bufferedData.Substring(endIndex + 1);
                        }
                        else if (bufferedData.IndexOf(linkMarker) == 1)
                        {
                            bufferedData = bufferedData.Substring(endIndex + 1);
                            result = true;
                        }
                        else
                        {
                            bufferedData = bufferedData.Substring(endIndex + 1);
                        }
                    }
                    else
                    {
                        Clear();
                    }
                }
                else
                {
                    Clear();
                }
            }
            return result;
        }

        public void Clear()
        {
            bufferedData = "";
        }
    }
}
