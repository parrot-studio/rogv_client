ROGv - Forts Watching System : Client
===============

Description
---------------
ROのGvGにおいて、「現状」を把握するためのシステム「ROGv」のクライアント側です。
以下の技術が使われています。

- C#
- .NET Framework 4(Full)

Usage
---------------
- Serverを設置する

https://github.com/parrot-studio/rogv_server

- Serverの情報を入力してテストする
- ROのChatディレクトリを指定
- RO内で作ったGvログ用タブ名を指定する

- 前回のラストログを読み込む
- 監視を開始する
- RO内で"/savechat"

Sample Site
---------------
http://parrot-studio.com/rogvs/

EAQ(Expected Asked Questions)
---------------
### どうやってコンパイルするのですか？

http://www.microsoft.com/japan/msdn/vstudio/express/

### 起動するとエラーになります

http://www.microsoft.com/downloads/ja-jp/details.aspx?FamilyID=0a391abd-25c1-4fc0-919f-b21f31ab88b7&displaylang=ja

### 不正ツールに当たりませんか？

私は当たらないように構築したつもりですが、
最終的に判断するのは運営です。
サポートに聞いたところ、「答えられない」と言われました。

あなた自身で判断できるようにソースを公開したわけで、判断はお任せします。

ChangeLog
---------------
#### ver1.1
- ログ読み込み時にサーバから最新データを取得するように修正
- ログの手動読み込みの目的を変更
- 割り込みボタンを追加

#### ver1.0
- 公開版

License
---------------
The MIT License

see LICENSE file for detail

Author
---------------
ぱろっと(@parrot_studio / parrot.studio.dev at gmail.com)
