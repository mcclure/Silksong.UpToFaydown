<h1>UpToFaydown</h1>

After player has Faydown Cloak, it will only trigger on up+jump, not regular jump inputs. This reserves regular jump for the Drifter's Cloak/Float.

There is one configuration key which turns the effect on and off.

<h1>Source:</h1>

GitHub: <a href = "https://github.com/mcclure/Silksong.UpToFaydown">https://github.com/mcclure/Silksong.UpToFaydown</a>

This is based on <a href="https://github.com/DemoJameson/Silksong.MakeFloatGreatAgain">a mod by DemoJameson</a> which does the opposite (forces hover when down is held). I release my changes to this plugin as [0-BSD](https://opensource.org/license/0bsd) (public domain) but DemoJameson's license on the basic code still applies.

The icon is created from a screenshot of https://gamepadviewer.com .

<h1>To build:</h1>

I use a `makezip.sh` to make the zip but you probably need to change "dotnet.exe" to "dotnet" for it to run on your machine.

<h1>To install:</h1>

<h3>Thunderstore:</h3>
It should all be handled for you auto-magically.

<h3>Manual:</h3>
First install BepInEx to your Silksong folder,
(note: this will break how thunderstore does things)

You can find it at
https://github.com/BepInEx/BepInEx/releases
latest stable is currently 5.4.23.4

After unzipping, run the game once, so that the BepInEx folder structure generates
(ie: there's folders in there apart from just `core`)

Then pull this DLL, or folder including the dll in to
<code>Hollow Knight Silksong\BepInEx\plugins</code>
