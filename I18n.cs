using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

public static class I18n
{
    public static string CurrentLanguage { get; private set; } = "en";

    // 言語ロード
    public static void Set(string lang)
    {
        CurrentLanguage = lang;
    }

    // 翻訳を取得（見つからなければキーを返す）
    public static string T(string key)
    {
        switch (CurrentLanguage)
        {
            case "日本語":
                switch (key)
                {
                    case "api_title":
                        return "Advanced A-B loop";
                    case "description":
                        return "高度なA-Bリピートの機能を提供します。";
                    case "hotkeyStartTime":
                        return "A-Bリピート:リピート開始地点の保存";
                    case "hotkeyEndTime":
                        return "A-Bリピート:リピート終了地点の保存";
                    case "hotkeyToggleABLoop":
                        return "A-Bリピート:A-Bリピートの有効/無効化";
                    case "hotkeyGoNextMeasure":
                        return "A-Bリピート:次の小節へ移動";
                    case "hotkeyGoPreviousMeasure":
                        return "A-Bリピート:前の小節へ移動";
                    case "hotkeyGoNextBeat":
                        return "A-Bリピート:一拍進める";
                    case "hotkeyGoPreviousBeat":
                        return "A-Bリピート:一拍戻る";

                    case "labelLoopStart":
                        return "リピート開始地点タグ名:";
                    case "labelLoopEnd":
                        return "リピート終了地点タグ名:";
                    case "labelLoopCounterTag":
                        return "リピート回数タグ名:";
                    case "labelDefaultLoopCount":
                        return "デフォルトリピート回数:";
                    case "labelLoopCountUnit":
                        return "回";
                    case "labelLoopCountTips":
                        return "2以上の数値を入力するとその回数リピートします。\n1を入力するとリピートせず通常通り再生されます。\n0を入力すると無限リピートします。";
                    case "labelAutoLoopToggle":
                        return "自動的にA-Bリピートを起動する:";
                    case "labelAutoLoopToggleTips":
                        return "有効にすると、musicBee起動時にA-Bリピートが自動的に有効化されます。";
                    case "labelLanguage":
                        return "言語:";
                    case "labelLanguageTips":
                        return "一部再起動必要";
                    case "labelIncrecasePlayCount":
                        return "A-Bリピート後に再生回数を増加させる:";
                    case "labelIncrecasePlayCountTips":
                        return "有効にすると、リピート時に再生回数を増やすようにします。\nリピート区間が極端に短い場合の再生回数異常増加を防ぐため、30秒以内の場合再生回数は増加しません。";
                    case "labelOffset":
                        return "オフセット (ms):";
                    case "labelOffsetUnit":
                        return "ms";
                    case "labelOffsetTips":
                        return "リピート終了地点の検出にオフセットを指定できます。値を大きくすればするほどリピート位置が後ろにずれます。\nプログラム上で取得した再生時間と実際の再生時間のずれを修正するためです。\n既存の値は作者の環境に基づいています。";

                    default:
                        return "エラー:" + key + "が見つかりません。これが見えたら開発者に報告してもらえると助かります。";
                }
            default:
                switch (key)
                {
                    case "api_title":
                        return "Advanced A-B loop";
                    case "description":
                        return "Provides advanced A-B loop functionality.";
                    case "hotkeyStartTime":
                        return "A-B loop:Save start time";
                    case "hotkeyEndTime":
                        return "A-B loop:Save end time";
                    case "hotkeyToggleABLoop":
                        return "A-B loop:Toggle A-B Looping";
                    case "hotkeyGoNextMeasure":
                        return "A-B loop:Go to next measure";
                    case "hotkeyGoPreviousMeasure":
                        return "A-B loop:Go to previous measure";
                    case "hotkeyGoNextBeat":
                        return "A-B loop:Go to next beat";
                    case "hotkeyGoPreviousBeat":
                        return "A-B loop:Go to previous beat";

                    case "labelLoopStart":
                        return "Loop start tag name:";
                    case "labelLoopEnd":
                        return "Loop end tag name:";
                    case "labelLoopCounterTag":
                        return "Loop counter tag name:";
                    case "labelDefaultLoopCount":
                        return "Default loop count:";
                    case "labelLoopCountUnit":
                        return "times";
                    case "labelLoopCountTips":
                        return "A loop count of 0 creates an infinite loop, 1 plays normally, and 2 or higher loops the specified number of times.\nIf no loop count is specified in the tag, this setting takes precedence.";
                    case "labelAutoLoopToggle":
                        return "Auto A-B Loop toggle:";
                    case "labelAutoLoopToggleTips":
                        return "If enabled, A-B looping will be automatically activated when musicBee starts.";
                    case "labelLanguage":
                        return "Language:";
                    case "labelLanguageTips":
                        return "Some changes may require a restart.";
                    case "labelIncrecasePlayCount":
                        return "Increase play count after A-B loop:";
                    case "labelIncrecasePlayCountTips":
                        return "If enabled, the play count of the track will be increased after completing the A-B loop.\nTo prevent abnormal increases in playback counts due to short-loop playback,\nwe only increment the count when playback continues for 30 seconds or longer.";
                    case "labelOffset":
                        return "Offset (ms):";
                    case "labelOffsetUnit":
                        return "ms";
                    case "labelOffsetTips":
                        return "You can specify the offset for loop end detection. The larger the value, the further back the loop position shifts.\nThis is necessary to eliminate discrepancies between the current playback time obtained by the program and the actual playback time.\nThe existing value is based on the author's environment.";

                    default:
                        return "ERROR:" + key + "is not found. If you see this, please let the developer know.";
                }
        }
    }
}