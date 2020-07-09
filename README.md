# PocketSphinxUnityDemo
A sample Unity project showing how to use PocketSphinx.

## Why was this created? 
When I stumbled across [CMUSphinx](https://cmusphinx.github.io/) and [PocketSphinx](https://github.com/cmusphinx/pocketsphinx), I was amazed at the capabilities these programs offered. However, there's very little documentation for anyone looking to implement this system within Unity. The only publicly available Unity Demo was created by [nshmyrev](https://github.com/cmusphinx/pocketsphinx-unity-demo), and hasn't been updated in quite some time. This project is an improved adaptation of his demo project. 

## General notes:
- You should be able to achieve basic speech recognition within Unity using this project. (Simple commands like "open folder" will work) However if you would like to recognize longer sentences, please look into [adapting the default acoustic model](https://cmusphinx.github.io/wiki/tutorialadapt/) or [building your own language model](https://cmusphinx.github.io/wiki/tutoriallm/).
- Keyphrases are cAsE SenSitiVe! When entering keyphrases and using the default 'en-us' model enter them as lowercase.
- This library / project does support VR platforms! (Tested with Oculus Quest) 
- The great thing with PocketSphinx is that you can have an offline / on-board speech recognition solution. (My particular use case was an offline solution for the Oculus Quest)

## This project supports and includes libraries for the following platforms:
- Windows x86 & x64
- OSX
- Android
- iOS

## Installation (Tested in Unity 2018.4.13f1 LTS)
Simply clone this project and open it in your version of Unity. This Project uses Git Large Files Support (LFS). Downloading a zip file using the green button on Github **does not work**. You must clone the Project with a version of git that has LFS. 

You can download Git LFS here: https://git-lfs.github.com/.

## TODO:
- ~~Add an example showing how to use a custom language model.~~
- ~~Add an example showing how to handle multiple keywords with KWS.~~
- (?) Add video example showing in-game use.

## Other great resources:
- [CMUSphinx](https://cmusphinx.github.io/)
- [Sophie's Blog - Python w/ PocketSphinx](http://blog.justsophie.com/python-speech-to-text-with-pocketsphinx/)

## Thanks!
Thanks for checking out my PocketSphinx Unity Demo, if you have any questions open a GitHub issue. :)
