using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
using System.Timers;

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
        private string loopCounterTag = "";
        private int loopCountDef = 0;
        private bool isIncrecasePlayCount = false;
        private int offsetMs = 430;

        private string startTagTemp = "";
        private string endTagTemp = "";
        private string echoTagTemp = "";
        private int echoTimeDefTemp = 0;
        private string loopCounterTagTemp = "";
        private int loopCountDefTemp = 0;
        private bool autoLoopToggleTemp = false;
        private string languageTemp = "English";
        private bool isIncrecasePlayCountTemp = false;
        private int offsetMsTemp = 450;

        private int loopStartMs = 0;
        private int loopEndMs = 0;
        private int echoTimeMs = 0;
        private float BPM = 120.0f;
        private int loopCounter = 0;

        private bool isLoopEnabled = false;
        private bool isTempoMulti1 = false;

        private int playTimeInLoop = 0;
        private int playTimeNeedToIncrease = 30; // 再生回数を増加させるまでのループ内再生時間（秒）

        private string language = "English"; // 設定を読み取る方法がわからなかったので手動で設定。変更するとビルドした際の言語が変わる。


        private System.Timers.Timer checkTimer = new System.Timers.Timer();
        private System.Timers.Timer loopTimeCountTimer = new System.Timers.Timer();
        private System.Timers.Timer logTimer = new System.Timers.Timer();
        private System.Timers.Timer checkLongPressTimer = new System.Timers.Timer();

        private bool isPlaying = false;
        private bool isLongPress = false;

        private string debugLogPath = Path.Combine("C:\\workspace\\musicbeeplugins\\log", "plugin_error.txt");

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            File.WriteAllText(debugLogPath, "test");

            mbApiInterface = new MusicBeeApiInterface();
            try
            {
                mbApiInterface = (MusicBeeApiInterface)Marshal.PtrToStructure(apiInterfacePtr, typeof(MusicBeeApiInterface));
                about.PluginInfoVersion = PluginInfoVersion;
                about.Name = I18n.T("api_title");
                about.Description = I18n.T("description");
                about.Author = "bobshroom";
                about.TargetApplication = "";   // current only applies to artwork, lyrics or instant messenger name that appears in the provider drop down selector or target Instant Messenger
                about.Type = PluginType.General;
                about.VersionMajor = 1;  // your plugin version 破壊的変更
                about.VersionMinor = 0; // your plugin version 機能追加
                about.Revision = 0; // your plugin version バグ修正
                about.MinInterfaceVersion = MinInterfaceVersion;
                about.MinApiRevision = MinApiRevision;
                about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
                about.ConfigurationPanelHeight = 200;   // height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function

                checkTimer.AutoReset = false;
                checkTimer.Elapsed += (sender, e) => checkLooping();

                loopTimeCountTimer.Interval = 1000;
                loopTimeCountTimer.AutoReset = true;
                loopTimeCountTimer.Elapsed += (sender, e) => countLoopTime();

                logTimer.Interval = 1000;
                logTimer.AutoReset = true;
                logTimer.Elapsed += (sender, e) => log();
                //logTimer.Start();

                checkLongPressTimer.Interval = 50;
                checkLongPressTimer.AutoReset = false;
                checkLongPressTimer.Elapsed += (sender, e) => checkLongPress();





                // 設定の読み取り
                string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
                string jsonFile = Path.Combine(dataPath, "advanceABLoopSettings.json");

                var loaded = JsonKeyValueStore.LoadKeyValue(jsonFile);

                startTag = loaded.ContainsKey("StartTag") ? loaded["StartTag"] : "Custom14";
                endTag = loaded.ContainsKey("EndTag") ? loaded["EndTag"] : "Custom15";
                //echoTag = loaded.ContainsKey("EchoTag") ? loaded["EchoTag"] : "Custom4";
                //echoTimeDef = loaded.ContainsKey("EchoTimeDef") ? (int)float.Parse(loaded["EchoTimeDef"]) : 0;
                loopCounterTag = loaded.ContainsKey("LoopCounterTag") ? loaded["LoopCounterTag"] : "Custom16";
                loopCountDef = loaded.ContainsKey("LoopCountDef") ? int.Parse(loaded["LoopCountDef"]) : 0;
                isLoopEnabled = loaded.ContainsKey("AutoLoopToggle") ? bool.Parse(loaded["AutoLoopToggle"]) : false;
                autoLoopToggleTemp = isLoopEnabled;
                language = loaded.ContainsKey("Language") ? loaded["Language"] : "English";
                isIncrecasePlayCount = loaded.ContainsKey("IsIncrecasePlayCount") ? bool.Parse(loaded["IsIncrecasePlayCount"]) : false;
                offsetMs = loaded.ContainsKey("OffsetMs") ? int.Parse(loaded["OffsetMs"]) : 430;
                I18n.Set(language);

                about.Name = I18n.T("api_title");
                about.Description = I18n.T("description");

                // 言語を変えるとホットキーも変わるようです。極力言語の変更は避けることをおすすめします。
                mbApiInterface.MB_AddMenuItem("SaveStartTime", I18n.T("hotkeyStartTime"), saveStartTime);
                mbApiInterface.MB_AddMenuItem("SaveEndTime", I18n.T("hotkeyEndTime"), saveEndTime);
                mbApiInterface.MB_AddMenuItem("ToggleLooping", I18n.T("hotkeyToggleABLoop"), toggleLooping);
                mbApiInterface.MB_AddMenuItem("GoToNextMeasure", I18n.T("hotkeyGoNextMeasure"), nextStep);
                mbApiInterface.MB_AddMenuItem("GoToPreviousMeasure", I18n.T("hotkeyGoPreviousMeasure"), previousStep);
                //mbApiInterface.MB_AddMenuItem("GoToNextBeat", I18n.T("hotkeyGoNextBeat"), nextBeat);
                //mbApiInterface.MB_AddMenuItem("GoToPreviousMeasure", I18n.T("hotkeyGoPreviousBeat"), previousBeat);
                //mbApiInterface.MB_AddMenuItem("Debug", "Debug", debug);

            }
            catch (Exception ex)
            {
                //File.WriteAllText(debugLogPath, "error:" + ex.ToString());
            }
            Debug.WriteLine("初期化成功");

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


                int index = 0;

                var layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.ColumnCount = 3;
                layout.RowCount = 15;
                layout.AutoSize = true;
                layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                Label loopStartLabel = new Label();
                loopStartLabel.AutoSize = true;
                loopStartLabel.Text = I18n.T("labelLoopStart");
                loopStartLabel.Anchor = AnchorStyles.Left;
                layout.Controls.Add(loopStartLabel, 0, index);

                TextBox loopStartTextBox = new TextBox();
                loopStartTextBox.Text = startTag;
                startTagTemp = startTag;
                layout.Controls.Add(loopStartTextBox, 1, index);

                index++;

                Label loopEndLabel = new Label();
                loopEndLabel.AutoSize = true;
                loopEndLabel.Text = I18n.T("labelLoopEnd");
                loopEndLabel.Anchor = AnchorStyles.Left;
                layout.Controls.Add(loopEndLabel, 0, index);

                TextBox loopEndTextBox = new TextBox();
                loopEndTextBox.Text = endTag;
                endTagTemp = endTag;
                layout.Controls.Add(loopEndTextBox, 1, index);

                index++;

                Label loopCounterTagLabel = new Label();
                loopCounterTagLabel.AutoSize = true;
                loopCounterTagLabel.Text = I18n.T("labelLoopCounterTag");
                loopCounterTagLabel.Anchor = AnchorStyles.Left;
                layout.Controls.Add(loopCounterTagLabel, 0, index);

                TextBox loopCounterTagTextBox = new TextBox();
                loopCounterTagTextBox.Text = loopCounterTag;
                loopCounterTagTemp = loopCounterTag;
                layout.Controls.Add(loopCounterTagTextBox, 1, index);

                index++;

                Label loopCountDefLabel = new Label();
                loopCountDefLabel.AutoSize = true;
                loopCountDefLabel.Text = I18n.T("labelDefaultLoopCount");
                loopCountDefLabel.Anchor = AnchorStyles.Left;
                layout.Controls.Add(loopCountDefLabel, 0, index);

                NumericUpDown loopCountDefNumberBox = new NumericUpDown();
                //loopCountDefNumberBox.Anchor = AnchorStyles.Right;
                loopCountDefNumberBox.Maximum = 99999;
                loopCountDefNumberBox.Value = Math.Min(loopCountDef, 99999);
                loopCountDefTemp = loopCountDef;
                layout.Controls.Add(loopCountDefNumberBox, 1, index);

                Label loopCountDefLabelUnit = new Label();
                loopCountDefLabelUnit.AutoSize = true;
                loopCountDefLabelUnit.Text = I18n.T("labelLoopCountUnit");
                loopCountDefLabelUnit.Anchor = AnchorStyles.Left;
                layout.Controls.Add(loopCountDefLabelUnit, 2, index);

                index++;

                Label loopTips = new Label();
                loopTips.AutoSize = true;
                loopTips.Text = I18n.T("labelLoopCountTips");
                layout.Controls.Add(loopTips, 0, index);
                layout.SetColumnSpan(loopTips, 3);

                index++;

                Label breakLabel = new Label();
                breakLabel.AutoSize = true;
                breakLabel.Text = " ";
                layout.Controls.Add(breakLabel, 0, index);

                index++;

                Label autoLoopToggleLabel = new Label();
                autoLoopToggleLabel.AutoSize = true;
                autoLoopToggleLabel.Text = I18n.T("labelAutoLoopToggle");
                autoLoopToggleLabel.Anchor = AnchorStyles.Left;
                layout.Controls.Add(autoLoopToggleLabel, 0, index);

                CheckBox autoLoopToggleCheckBox = new CheckBox();
                autoLoopToggleCheckBox.Checked = autoLoopToggleTemp;
                layout.Controls.Add(autoLoopToggleCheckBox, 1, index);

                index++;

                Label autoLoopToggleTips = new Label();
                autoLoopToggleTips.AutoSize = true;
                autoLoopToggleTips.Text = I18n.T("labelAutoLoopToggleTips");
                layout.Controls.Add(autoLoopToggleTips, 0, index);
                layout.SetColumnSpan(autoLoopToggleTips, 3);

                index++;

                Label languageLabel = new Label();
                languageLabel.AutoSize = true;
                languageLabel.Text = I18n.T("labelLanguage");
                languageLabel.Anchor = AnchorStyles.Left;
                layout.Controls.Add(languageLabel, 0, index);

                ComboBox languageComboBox = new ComboBox();
                languageComboBox.Items.Add("English");
                languageComboBox.Items.Add("日本語");
                languageComboBox.SelectedItem = language;
                languageTemp = language;
                layout.Controls.Add(languageComboBox, 1, index);

                Label languageTips = new Label();
                languageTips.AutoSize = true;
                languageTips.Anchor = AnchorStyles.Left;
                languageTips.Text = I18n.T("labelLanguageTips");
                layout.Controls.Add(languageTips, 2, index);

                index++;

                layout.Controls.Add(breakLabel, 0, index);

                index++;

                Label isIncrecasePlayCountLabel = new Label();
                isIncrecasePlayCountLabel.AutoSize = true;
                isIncrecasePlayCountLabel.Text = I18n.T("labelIncrecasePlayCount");
                isIncrecasePlayCountLabel.Anchor = AnchorStyles.Left;
                layout.Controls.Add(isIncrecasePlayCountLabel, 0, index);

                CheckBox isIncrecasePlayCountCheckBox = new CheckBox();
                isIncrecasePlayCountCheckBox.Checked = isIncrecasePlayCount;
                isIncrecasePlayCountTemp = isIncrecasePlayCount;
                layout.Controls.Add(isIncrecasePlayCountCheckBox, 1, index);

                index++;

                Label isIncrecasePlayCountTips = new Label();
                isIncrecasePlayCountTips.AutoSize = true;
                isIncrecasePlayCountTips.Text = I18n.T("labelIncrecasePlayCountTips");
                layout.Controls.Add(isIncrecasePlayCountTips, 0, index);
                layout.SetColumnSpan(isIncrecasePlayCountTips, 3);

                index++;

                layout.Controls.Add(breakLabel, 0, index);

                index++;

                Label offsetMsLabel = new Label();
                offsetMsLabel.AutoSize = true;
                offsetMsLabel.Text = I18n.T("labelOffset");
                offsetMsLabel.Anchor = AnchorStyles.Left;
                layout.Controls.Add(offsetMsLabel, 0, index);

                NumericUpDown offsetMsNumberBox = new NumericUpDown();
                offsetMsNumberBox.Minimum = -9999;
                offsetMsNumberBox.Maximum = 9999;
                offsetMsNumberBox.Value = Math.Min(Math.Max(offsetMs, -9999), 9999);
                offsetMsTemp = offsetMs;
                layout.Controls.Add(offsetMsNumberBox, 1, index);

                Label offsetMsLabelUnit = new Label();
                offsetMsLabelUnit.AutoSize = true;
                offsetMsLabelUnit.Text = "ms";
                offsetMsLabelUnit.Anchor = AnchorStyles.Left;
                layout.Controls.Add(offsetMsLabelUnit, 2, index);

                index++;

                Label offsetTipsLabel = new Label();
                offsetTipsLabel.AutoSize = true;
                offsetTipsLabel.Text = I18n.T("labelOffsetTips");
                layout.Controls.Add(offsetTipsLabel, 0, index);
                layout.SetColumnSpan(offsetTipsLabel, 3);



                /*Label echoLabel = new Label();
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
                echoTimeNubmerBox.Anchor = AnchorStyles.Right;
                echoTimeNumberBox.Maximum = 99999;
                echoTimeNumberBox.Value = Math.Min(echoTimeDef, 99999);
                echoTimeDefTemp = echoTimeDef;
                layout.Controls.Add(echoTimeNumberBox, 1, 3);

                Label echoTimeLabelUnit = new Label();
                echoTimeLabelUnit.AutoSize = true;
                echoTimeLabelUnit.Text = "ms";
                layout.Controls.Add(echoTimeLabelUnit, 2, 3);*/

                configPanel.AutoScroll = true;
                configPanel.AutoScrollMinSize = new Size(1300, 600);

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
                /*echoTagTextBox.TextChanged += (s, e) =>
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
                };*/
                loopCounterTagTextBox.TextChanged += (s, e) =>
                {
                    if (Enum.TryParse<MetaDataType>(loopCounterTagTextBox.Text, out MetaDataType result))
                    {
                        loopCounterTagTextBox.BackColor = Color.White;
                        loopCounterTagTemp = loopCounterTagTextBox.Text;
                    }
                    else
                    {
                        loopCounterTagTextBox.BackColor = Color.Red;
                    }
                };
                loopCountDefNumberBox.ValueChanged += (s, e) =>
                {
                    loopCountDefTemp = (int)loopCountDefNumberBox.Value;
                };
                autoLoopToggleCheckBox.CheckedChanged += (s, e) =>
                {
                    autoLoopToggleTemp = autoLoopToggleCheckBox.Checked;
                };
                languageComboBox.SelectedIndexChanged += (s, e) =>
                {
                    languageTemp = (string)languageComboBox.SelectedItem;
                };
                isIncrecasePlayCountCheckBox.CheckedChanged += (s, e) =>
                {
                    isIncrecasePlayCountTemp = isIncrecasePlayCountCheckBox.Checked;
                };
                offsetMsNumberBox.ValueChanged += (s, e) =>
                {
                    offsetMsTemp = (int)offsetMsNumberBox.Value;
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
            string jsonFile = Path.Combine(dataPath, "advanceABLoopSettings.json");

            startTag = startTagTemp;
            endTag = endTagTemp;
            //echoTag = echoTagTemp;
            //echoTimeDef = echoTimeDefTemp;
            loopCounterTag = loopCounterTagTemp;
            loopCountDef = loopCountDefTemp;
            language = languageTemp;
            isIncrecasePlayCount = isIncrecasePlayCountTemp;
            offsetMs = offsetMsTemp;

            var settings = new Dictionary<string, string>
            {
                { "StartTag", startTag },
                { "EndTag", endTag },
                //{ "EchoTag", echoTag },
                //{ "EchoTimeDef", echoTimeDef.ToString() },
                { "LoopCounterTag", loopCounterTag },
                { "LoopCountDef", loopCountDef.ToString() },
                { "AutoLoopToggle", autoLoopToggleTemp.ToString()   },
                { "Language", language   },
                { "IsIncrecasePlayCount", isIncrecasePlayCount.ToString() },
                { "OffsetMs", offsetMs.ToString()   }
            };
            I18n.Set(language);

            JsonKeyValueStore.SaveKeyValue(jsonFile, settings);
        }

        // MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
        public void Close(PluginCloseReason reason)
        {
        }

        // uninstall this plugin - clean up any persisted files
        public void Uninstall()
        {
            try
            {
                string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
                string settingsFile = Path.Combine(dataPath, "advanceABLoopSettings.json");

                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }
            }
            catch
            {
                // 失敗しても何もしない（アンインストールを妨げない）
            }
        }

        // receive event notifications from MusicBee
        // you need to set about.ReceiveNotificationFlags = PlayerEvents to receive all notifications, and not just the startup event
        public async Task ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            // perform some action depending on the notification type
            switch (type)
            {
                case NotificationType.PluginStartup:
                    /*/ perform startup initialisation
                    switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:
                            Debug.WriteLine("MusicBee is playing");
                            break;
                        case PlayState.Paused:
                            Debug.WriteLine("MusicBee is paused");
                            // ...
                            break;
                    }*/
                    GetTagInfo();
                    StopCheckTimer();
                    break;

                case NotificationType.PlayStateChanged:
                    switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:
                            isPlaying = true;
                            GetTagInfo();
                            SetCheckTimer();
                            loopTimeCountTimer.Start();
                            break;
                        case PlayState.Paused:
                            isPlaying = false;
                            StopCheckTimer();
                            loopTimeCountTimer.Stop();
                            break;
                    }
                    break;

                case NotificationType.TrackChanged:
                    GetTagInfo();
                    SetCheckTimer();
                    break;

                case NotificationType.TagsChanged:
                    GetTagInfo();
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
            return returnValue;
        }

        private void GetTagInfo()
        {
            // プラグイン専用のタグから情報を取得
            Debug.WriteLine("タグ情報取得開始");
            string loopStartStr = mbApiInterface.NowPlaying_GetFileTag((MetaDataType)Enum.Parse(typeof(MetaDataType), startTag));
            string loopEndStr = mbApiInterface.NowPlaying_GetFileTag((MetaDataType)Enum.Parse(typeof(MetaDataType), endTag));
            //string echoStr = mbApiInterface.NowPlaying_GetFileTag((MetaDataType)Enum.Parse(typeof(MetaDataType), echoTag));
            string loopCounterStr = mbApiInterface.NowPlaying_GetFileTag((MetaDataType)Enum.Parse(typeof(MetaDataType), loopCounterTag));
            string BPMStr = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.BeatsPerMin);
            loopStartMs = TimeStringToMilliseconds(loopStartStr);
            loopEndMs = TimeStringToMilliseconds(loopEndStr);

            Debug.WriteLine("変換開始");

            /*if (string.IsNullOrEmpty(echoStr))
            {
                echoTimeMs = echoTimeDef;
            }
            else
            {
                echoTimeMs = int.Parse(echoStr);
            }*/

            if (string.IsNullOrEmpty(loopCounterStr))
            {
                loopCounter = loopCountDef;
            }
            else
            {
                loopCounter = int.Parse(loopCounterStr);
            }
            if (string.IsNullOrEmpty(BPMStr))
            {
                BPM = 120.0f;
            }
            else
            {
                if (!float.TryParse(BPMStr, out BPM))
                {
                    BPM = 120.0f;
                }
            }
            Debug.WriteLine("タグ取得終了");
        }

        private bool IsOutLoopRegion(int currentPositionMs)
        {
            if (loopStartMs < loopEndMs)
            {
                if (currentPositionMs > loopEndMs)
                {
                    return true;
                }
            }
            return false;
        }

        private void checkLooping()
        {
            if (isLoopEnabled)
            {
                int currentPositionMs = mbApiInterface.Player_GetPosition();
                //currentPositionMs -= 120;
                if (IsOutLoopRegion(currentPositionMs - offsetMs) && loopCounter != 1)
                {
                    Debug.WriteLine("ループ前の位置       :" + loopEndMs.ToString());
                    Debug.WriteLine("ループ前の実際の位置 :" + mbApiInterface.Player_GetPosition().ToString());
                    mbApiInterface.Player_SetPosition(loopStartMs);
                    //offsetMs = loopStartMs - mbApiInterface.Player_GetPosition();
                    Debug.WriteLine("ループ後の位置       :" + loopStartMs.ToString());
                    Debug.WriteLine("ループ後の実際の位置 :" + mbApiInterface.Player_GetPosition().ToString());
                    loopCounter--;
                    if (isIncrecasePlayCount && playTimeInLoop >= playTimeNeedToIncrease)
                    {
                        mbApiInterface.Player_UpdatePlayStatistics(mbApiInterface.NowPlaying_GetFileUrl(), PlayStatisticType.IncreasePlayCount, false);
                    }
                    playTimeInLoop = 0;
                }
                SetCheckTimer();
                return;
            }
            StopCheckTimer();
        }
        private int DelayMilliseconds()
        {
            int delayMs = loopEndMs - mbApiInterface.Player_GetPosition();
            //delayMs += 120;
            if (delayMs <= 0)
            {
                delayMs = 1;
            }
            return delayMs;
        }

        private void SetCheckTimer()
        {
            //Debug.WriteLine("SetCheckTimer");
            checkTimer.Interval = Math.Max((int)(DelayMilliseconds() * 0.3), 1);
            checkTimer.Start();
        }

        private void StopCheckTimer()
        {
            //Debug.WriteLine("StopCheckTimer");
            checkTimer.Stop();
        }

        private void countLoopTime()
        {
            if (isPlaying && isLoopEnabled && isIncrecasePlayCount) playTimeInLoop = Math.Min(playTimeInLoop + 1, playTimeNeedToIncrease);
        }

        private void saveStartTime(object sender, EventArgs args)
        {
            int currentPositionMs = mbApiInterface.Player_GetPosition();
            string timeStr = MillisecondsToTimeString(currentPositionMs);
            mbApiInterface.Library_SetFileTag(mbApiInterface.NowPlaying_GetFileUrl(), (MetaDataType)Enum.Parse(typeof(MetaDataType), startTag), timeStr);
            mbApiInterface.Library_CommitTagsToFile(mbApiInterface.NowPlaying_GetFileUrl());
            loopStartMs = currentPositionMs;
        }
        private void saveEndTime(object sender, EventArgs args)
        {
            int currentPositionMs = mbApiInterface.Player_GetPosition();
            string timeStr = MillisecondsToTimeString(currentPositionMs);
            mbApiInterface.Library_SetFileTag(mbApiInterface.NowPlaying_GetFileUrl(), (MetaDataType)Enum.Parse(typeof(MetaDataType), endTag), timeStr);
            mbApiInterface.Library_CommitTagsToFile(mbApiInterface.NowPlaying_GetFileUrl());
            loopEndMs = currentPositionMs;
            SetCheckTimer();
            Debug.WriteLine("ループ終了地点を保存:" + currentPositionMs);
        }
        private void toggleLooping(object sender, EventArgs args)
        {
            isLoopEnabled = !isLoopEnabled;
            if (isLoopEnabled)
            {
                SetCheckTimer();
            }
            else
            {
                StopCheckTimer();
            }
        }
        private void nextStep(object sender, EventArgs args)
        {
            int beat = 4; // 4分音符
            int currentPositionMs = mbApiInterface.Player_GetPosition();
            if (!isLongPress)
            {
                if (isPlaying) currentPositionMs -= offsetMs + 0; // 補正
            }
            isLongPress = true;
            checkLongPressTimer.Start();
            double msPerBeat = 60000.0f / BPM;

            int d = loopStartMs;
            double L = msPerBeat * beat;
            int x = currentPositionMs;

            int nextMeasurePositionMs = (int)(d + (Math.Floor((x - d) / L) + 1) * L);

            mbApiInterface.Player_SetPosition(nextMeasurePositionMs);
            if (isLoopEnabled)
            {
                SetCheckTimer();
            }
        }

        private void nextBeat(object sender, EventArgs args)
        {
            int beat = 1; // 一拍
            int currentPositionMs = mbApiInterface.Player_GetPosition();
            if (!isLongPress)
            {
                if (isPlaying) currentPositionMs -= offsetMs + 0; // 補正
            }
            isLongPress = true;
            checkLongPressTimer.Start();
            double msPerBeat = 60000.0f / BPM;

            int d = loopStartMs;
            double L = msPerBeat * beat;
            int x = currentPositionMs;

            int nextMeasurePositionMs = (int)(d + (Math.Floor((x - d) / L) + 1) * L);

            mbApiInterface.Player_SetPosition(nextMeasurePositionMs);
            if (isLoopEnabled)
            {
                SetCheckTimer();
            }
        }

        private void previousStep(object sender, EventArgs args)
        {
            int beat = 4; // 4分音符
            int currentPositionMs = mbApiInterface.Player_GetPosition();
            if (isPlaying) currentPositionMs -= offsetMs; // 補正
            double msPerBeat = 60000.0f / BPM;

            int d = loopStartMs;
            double L = msPerBeat * beat;
            int x = currentPositionMs;
            int r = 500; // 誤差許容範囲500ms

            int s = (int)(d + Math.Floor((x - d) / L) * L);
            int nextMeasurePositionMs;
            Debug.WriteLine($"小節の頭: {s}, 現在位置: {x}, 差分: {x - s}");
            if (x - s <= r)
            {
                // 次の小節の頭から近い場合は前の小節へ
                nextMeasurePositionMs = (int)(s - L);
            }
            else
            {
                // そうでなければ今の小節の頭へ
                nextMeasurePositionMs = s;
            }
            mbApiInterface.Player_SetPosition(nextMeasurePositionMs);

            if (isLoopEnabled)
            {
                SetCheckTimer();
            }
        }
        private void previousBeat(object sender, EventArgs args)
        {
            int beat = 1; // 1拍
            int currentPositionMs = mbApiInterface.Player_GetPosition();
            if (isPlaying) currentPositionMs -= offsetMs; // 補正
            double msPerBeat = 60000.0f / BPM;

            int d = loopStartMs;
            double L = msPerBeat * beat;
            int x = currentPositionMs;
            int r = 500; // 誤差許容範囲500ms

            int s = (int)(d + Math.Floor((x - d) / L) * L);
            int nextMeasurePositionMs;
            Debug.WriteLine($"小節の頭: {s}, 現在位置: {x}, 差分: {x - s}");
            if (x - s <= r)
            {
                // 次の小節の頭から近い場合は前の小節へ
                nextMeasurePositionMs = (int)(s - L);
            }
            else
            {
                // そうでなければ今の小節の頭へ
                nextMeasurePositionMs = s;
            }
            mbApiInterface.Player_SetPosition(nextMeasurePositionMs);

            if (isLoopEnabled)
            {
                SetCheckTimer();
            }
        }

        private void checkLongPress()
        {
            isLongPress = false;
            checkLongPressTimer.Stop();
        }

        private void log()
        {
            Debug.WriteLine(isLoopEnabled);
        }
        private void debug(object sender, EventArgs args)
        {
            Debug.WriteLine("デバッグコマンド実行");
            int currentPositionMs = mbApiInterface.Player_GetPosition();
            Debug.WriteLine("現在の位置:" + currentPositionMs);
            /*mbApiInterface.Player_SetPosition(currentPositionMs);
            Debug.WriteLine("変更後の位置:" + mbApiInterface.Player_GetPosition());
            Debug.WriteLine("誤差" + (currentPositionMs - mbApiInterface.Player_GetPosition()).ToString() + "ms");*/
        }
    }
}