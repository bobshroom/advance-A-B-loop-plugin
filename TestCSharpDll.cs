using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();

        private string startTag = "";
        private string endTag = "";
        private string echoTag = "";
        private int echoTimeDef = 0;

        private string startTagTemp = "";
        private string endTagTemp = "";
        private string echoTagTemp = "";
        private int echoTimeDefTemp = 0;

        private int loopStartMs = 0;
        private int loopEndMs = 0;
        private int echoTimeMs = 0;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "advance A-B loop CS";
            about.Description = "test";
            about.Author = "Bob";
            about.TargetApplication = "";   // current only applies to artwork, lyrics or instant messenger name that appears in the provider drop down selector or target Instant Messenger
            about.Type = PluginType.General;
            about.VersionMajor = 1;  // your plugin version
            about.VersionMinor = 0;
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 200;   // height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function
            // 設定の読み取り
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            string jsonFile = Path.Combine(dataPath, "settings.json");

            var loaded = JsonKeyValueStore.LoadKeyValue(jsonFile);
            startTag = loaded.ContainsKey("StartTag") ? loaded["StartTag"] : "";
            endTag = loaded.ContainsKey("EndTag") ? loaded["EndTag"] : "";
            echoTag = loaded.ContainsKey("EchoTag") ? loaded["EchoTag"] : "";
            echoTimeDef = loaded.ContainsKey("EchoTimeDef") ? (int)float.Parse(loaded["EchoTimeDef"]) : 0;

            return about;
        }

        public bool Configure(IntPtr panelHandle)
        {
            // save any persistent settings in a sub-folder of this path
            // dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            // panelHandle will only be set if you set about.ConfigurationPanelHeight to a non-zero value
            // keep in mind the panel width is scaled according to the font the user has selected
            // if about.ConfigurationPanelHeight is set to 0, you can display your own popup window
            if (panelHandle != IntPtr.Zero)
            {
                Panel configPanel = (Panel)Panel.FromHandle(panelHandle);

                var layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.ColumnCount = 3;
                layout.RowCount = 4;
                layout.AutoSize = true;
                layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                Label loopStartLabel = new Label();
                loopStartLabel.AutoSize = true;
                loopStartLabel.Text = "Loop start tag name:";
                layout.Controls.Add(loopStartLabel, 0, 0);

                TextBox loopStartTextBox = new TextBox();
                loopStartTextBox.Text = startTag;
                startTagTemp = startTag;
                layout.Controls.Add(loopStartTextBox, 1, 0);

                Label loopEndLabel = new Label();
                loopEndLabel.AutoSize = true;
                loopEndLabel.Text = "Loop End tag name:";
                layout.Controls.Add(loopEndLabel, 0, 1);

                TextBox loopEndTextBox = new TextBox();
                loopEndTextBox.Text = endTag;
                endTagTemp = endTag;
                layout.Controls.Add(loopEndTextBox, 1, 1);

                Label echoLabel = new Label();
                echoLabel.AutoSize = true;
                echoLabel.Text = "Loop echo tag name:";
                layout.Controls.Add(echoLabel, 0, 2);

                TextBox echoTagTextBox = new TextBox();
                echoTagTextBox.Text = echoTag;
                echoTagTemp = echoTag;
                layout.Controls.Add(echoTagTextBox, 1, 2);

                Label echoTimeLabel = new Label();
                echoTimeLabel.AutoSize = true;
                echoTimeLabel.Text = "Default echo time:";
                layout.Controls.Add(echoTimeLabel, 0, 3);

                NumericUpDown echoTimeNumberBox = new NumericUpDown();
                //echoTimeNubmerBox.Anchor = AnchorStyles.Right;
                echoTimeNumberBox.Maximum = 99999;
                echoTimeNumberBox.Value = Math.Min(echoTimeDef, 99999);
                echoTimeDefTemp = echoTimeDef;
                layout.Controls.Add(echoTimeNumberBox, 1, 3);

                Label echoTimeLabelUnit = new Label();
                echoTimeLabelUnit.AutoSize = true;
                echoTimeLabelUnit.Text = "ms";
                layout.Controls.Add(echoTimeLabelUnit, 2, 3);

                configPanel.Controls.Add(layout);

                // はじめはボタンを押したら保存する使用だったが、手間が多く保存忘れの恐れがあるため無効化した
                /*Button saveButton = new Button();
                saveButton.Text = "save";
                saveButton.Location = new Point(0, 60);
                configPanel.Controls.Add(saveButton);*/

                loopStartTextBox.TextChanged += (s, e) =>
                {
                    if (Enum.TryParse<MetaDataType>(loopStartTextBox.Text, out MetaDataType result))
                    {
                        loopStartTextBox.BackColor = Color.White;
                        startTagTemp = loopStartTextBox.Text;
                    }
                    else
                    {
                        loopStartTextBox.BackColor = Color.Red;
                    }
                };
                loopEndTextBox.TextChanged += (s, e) =>
                {
                    if (Enum.TryParse<MetaDataType>(loopEndTextBox.Text, out MetaDataType result))
                    {
                        loopEndTextBox.BackColor = Color.White;
                        endTagTemp = loopEndTextBox.Text;
                    }
                    else
                    {
                        loopEndTextBox.BackColor = Color.Red;
                    }
                };
                echoTagTextBox.TextChanged += (s, e) =>
                {
                    if (Enum.TryParse<MetaDataType>(echoTagTextBox.Text, out MetaDataType result))
                    {
                        echoTagTextBox.BackColor = Color.White;
                        echoTagTemp = echoTagTextBox.Text;
                    }
                    else
                    {
                        echoTagTextBox.BackColor = Color.Red;
                    }
                };
                echoTimeNumberBox.ValueChanged += (s, e) =>
                {
                    echoTimeDefTemp = (int)echoTimeNumberBox.Value;
                };
            }
            return false;
        }

        // called by MusicBee when the user clicks Apply or Save in the MusicBee Preferences screen.
        // its up to you to figure out whether anything has changed and needs updating
        public void SaveSettings()
        {
            // save any persistent settings in a sub-folder of this path
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            string jsonFile = Path.Combine(dataPath, "settings.json");

            startTag = startTagTemp;
            endTag = endTagTemp;
            echoTag = echoTagTemp;
            echoTimeDef = echoTimeDefTemp;

            var settings = new Dictionary<string, string>
            {
                { "StartTag", startTag },
                { "EndTag", endTag },
                { "EchoTag", echoTag },
                { "EchoTimeDef", echoTimeDef.ToString() }
            };

            JsonKeyValueStore.SaveKeyValue(jsonFile, settings);
        }

        // MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
        public void Close(PluginCloseReason reason)
        {
        }

        // uninstall this plugin - clean up any persisted files
        public void Uninstall()
        {
        }

        // receive event notifications from MusicBee
        // you need to set about.ReceiveNotificationFlags = PlayerEvents to receive all notifications, and not just the startup event
        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            // perform some action depending on the notification type
            switch (type)
            {
                case NotificationType.PluginStartup:
                    // perform startup initialisation
                    switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:
                            Debug.WriteLine("MusicBee is playing");
                            break;
                        case PlayState.Paused:
                            Debug.WriteLine("MusicBee is paused");
                            // ...
                            break;
                    }
                    break;
                case NotificationType.TrackChanged:

                    string test = "Custom1";
                    if (Enum.TryParse<MetaDataType>(test, out MetaDataType result))
                    {
                        string value = mbApiInterface.NowPlaying_GetFileTag(result);
                        Debug.WriteLine("カスタム1: " + value);
                    }
                    else
                    {
                        Debug.WriteLine("タグの取得に失敗しました");
                    }
                    MillisecondsToTimeString(123456 + 10*60*1000);
                    break;
            }
        }

        // return an array of lyric or artwork provider names this plugin supports
        // the providers will be iterated through one by one and passed to the RetrieveLyrics/ RetrieveArtwork function in order set by the user in the MusicBee Tags(2) preferences screen until a match is found
        public string[] GetProviders()
        {
            return null;
        }

        // return lyrics for the requested artist/title from the requested provider
        // only required if PluginType = LyricsRetrieval
        // return null if no lyrics are found
        public string RetrieveLyrics(string sourceFileUrl, string artist, string trackTitle, string album, bool synchronisedPreferred, string provider)
        {
            return null;
        }

        // return Base64 string representation of the artwork binary data from the requested provider
        // only required if PluginType = ArtworkRetrieval
        // return null if no artwork is found
        public string RetrieveArtwork(string sourceFileUrl, string albumArtist, string album, string provider)
        {
            //Return Convert.ToBase64String(artworkBinaryData)
            return null;
        }

        // ここから自作
        class PluginSettings
        {
            public string UserName { get; set; }
            public int Volume { get; set; }
        }

        private int TimeStringToMilliseconds(string timeString)
        {
            // 小数点は . と , のどちらでも許容
            timeString = timeString.Replace(',', '.').Trim();

            // 分割
            var parts = timeString.Split(':');
            if (parts.Length == 2)
            {
                // mm:ss.fff
                // 例: "03:12.450"
                if (double.TryParse(parts[1], out double sec))
                {
                    int minutes = int.Parse(parts[0]);
                    var ts = new TimeSpan(0, 0, minutes, 0, 0) + TimeSpan.FromSeconds(sec);
                    return (int)ts.TotalMilliseconds;
                }
            }
            else if (parts.Length == 3)
            {
                // hh:mm:ss.fff
                // 例: "01:03:12.450"
                if (double.TryParse(parts[2], out double sec))
                {
                    int hours = int.Parse(parts[0]);
                    int minutes = int.Parse(parts[1]);
                    var ts = new TimeSpan(hours, minutes, 0) + TimeSpan.FromSeconds(sec);
                    return (int)ts.TotalMilliseconds;
                }
            }

            // フォーマットがおかしい場合
            return 0;
        }

        private string MillisecondsToTimeString(int milliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            string returnValue = "";
            if (timeSpan.Hours > 0)
            {
                returnValue = timeSpan.Hours.ToString() + ":";
            }
            returnValue += timeSpan.Minutes.ToString() + ":";
            returnValue += timeSpan.Seconds.ToString() + ".";
            returnValue += timeSpan.Milliseconds.ToString("D3");
            Debug.WriteLine(returnValue);
            return returnValue;
        }

        private void GetLoopPoints()
        {
            // ループポイントの取得
            string loopStartStr = mbApiInterface.NowPlaying_GetFileTag((MetaDataType)Enum.Parse(typeof(MetaDataType), startTag));
            string loopEndStr = mbApiInterface.NowPlaying_GetFileTag((MetaDataType)Enum.Parse(typeof(MetaDataType), endTag));
            string echoStr = mbApiInterface.NowPlaying_GetFileTag((MetaDataType)Enum.Parse(typeof(MetaDataType), echoTag));
            loopStartMs = TimeStringToMilliseconds(loopStartStr);
            loopEndMs = TimeStringToMilliseconds(loopEndStr);

            if (string.IsNullOrEmpty(echoStr))
            {
                echoTimeMs = echoTimeDef;
            }
            else
            {
                //echoTimeMs = echoStr;
            }
        }
    }
}