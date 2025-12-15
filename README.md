# Advanced A-B loop

A-B Loop plugin for MusicBee.

## Features
 - A-B Loop Function
 - Easy setup with hotkeys
 - Specify loop count
 - Measure-based navigation (BPM-based)

## Installation
1. Download the plugin files
2. Open the plugin settings screen in MusicBee
3. Click “Add Plugin...” and select the downloaded file.

## Usage
- Set the loop start tag (default: `Custom14`)
- Set the loop end tag (default: `Custom15`)
 - Toggle looping with hotkey

Hotkeys and tags can be customized in MusicBee settings.

### How to Use More Advanced Features
 - Change which tags are used to store loop start and end positions
 - Save loop start and end positions from the current playback position using hotkeys
 - Specify loop count:
 - `0` = infinite loop
 - `1` = play once (no loop)
 - `2+` = loop the specified number of times
 - Optionally increase play count during looping  
  (When enabled, scrobbling to Last.fm is also performed)
 - Automatically enable A-B looping when MusicBee starts
 - Measure-based navigation using BPM and loop start position  
  If the actual loop timing differs, adjust the offset value.

## Settings
 - Loop start tag name
 - Loop end tag name
 - Loop counter tag name
 - Default loop count
 - Auto A-B Loop toggle
 - Language
 - Increase play count after A-B loop
 - Offset (timing adjustment in milliseconds)

Settings are saved automatically.

## Hotkeys
 - Save start time
 - Save end time
 - Toggle A-B looping
 - Go to next measure
 - Go to previous measure

## Compatibility
- MusicBee 3.x
- Windows 10 / 11

## Version
- v1.0.0

## License
MIT License

## Author
bobshroom


# Advanced A-B loop

A-Bリピートの機能を追加するMusicBeeのプラグインです。

## 機能
 - A-Bリピート
 - ホットキーから簡単設定
 - 視聴回数の増加設定
 - BPMを基準とした小節間の移動

## 導入方法
1. プラグインのファイルをダウンロードする
2. MusicBeeでプラグインの設定画面を開く
3. 「プラグインを追加」をクリックし、ダウンロードしたファイルを選択

## 使い方
- リピート開始位置をタグで設定 (デフォルト: `Custom14`)
- リピート終了位置をタグで設定 (デフォルト: `Custom15`)
 - ホットキーからA-Bループを有効化

MusicBeeの設定からホットキーの設定をしてください。
また、リピート開始位置のタグなどは、プラグインの設定でカスタマイズできます。(Custom1など)

### より使いこなすには
 - リピートの開始位置と終了位置を保存するタグの名前を変更しわかりやすくする
 - ホットキーから、現在の再生位置をそのままリピート開始・終了位置のタグに書き込み可能
 - リピート回数の設定
 - `0` = 無限リピート
 - `1` = 通常再生(リピート無し)
 - `2以上` = 指定回数のリピート
 - リピートすると再生回数の増加を設定
  (有効化時、自動的にLast.fmにscrobbleされます)
 - MusicBee開始時にA-Bリピートを自動的に有効化可能
 - BPMとリピート開始位置から、小節間の移動をホットキーで可能(4/4拍子を想定)  
  設定したリピート時間と実際のリピート時間がずれる場合、オフセットの値を変更してください。

## 設定
 - リピート開始地点タグ名
 - リピート終了地点タグ名
 - リピート回数タグ名
 - デフォルトリピート回数(リピート回数が指定されていない場合こちらの値を使用します)
 - 自動的にA-Bリピートを起動するか
 - 言語設定(言語を変えるとホットキーがリセットされるので、再設定してください)
 - A-Bリピート後に再生回数を増加させるか
 - オフセット


## ホットキー
 - リピート開始地点の保存
 - リピート終了地点の保存
 - A-Bリピートの有効/無効化の切り替え
 - 次の小説に移動
 - 前の小説に移動

## 互換性
- MusicBee 3.x
- Windows 10 / 11

## バージョン
- v1.0.0

## ライセンス
MIT License

## 作者
bobshroom
