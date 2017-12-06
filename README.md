# Why forked
I wanted to add ice and lava layers, but got much more ideas in process!!

[![Waffle.io - Columns and their card count](https://badge.waffle.io/Nashet/Interactive-Erosion.svg?columns=all)](https://waffle.io/Nashet/Interactive-Erosion)

<img src="https://i.imgur.com/Bxg7RKQ.png" width="480">

Heavy screenshots:

https://i.imgur.com/FGxkEwy.jpg

https://i.imgur.com/kWZczeV.jpg

https://i.imgur.com/oLm71P8.jpg

# How to setup

Basicly you just need to install Unity 5.6.1 (other releases could be incompatible, free version is enough). Then open project in Unity. Then open scene in unity, it's on bottom of "project" tab. It should be ready.
One note - value of some publiс fields are overwritten by unity inspector, you'll see it after meeting with inspector.

I try to keep master branch stable more or less.
I build for default settings, Windows _86. Hopefully, there wouldn't be problems with shaders compilation for your platform.
Also, MS Visual Studio might be helpful

# Interactive-Erosion (original text)

This project is based on the excellent article Interactive Terrain Modeling Using Hydraulic Erosion. The Authors were also kind enough to publish the code in ShaderX7 which is what a lot of this Unity project is based on. I have made some changes and left some things out. 

The idea behind this project is to erode a height map through a series of natural processes to help create a more realistic height map. Three processes are applied to the height map each frame and can be classed as force-based erosion, dissolution-based erosion and material slippage. All of these processes are carried out on the GPU. The height map can be represented by a series of layers (4, one for each RGBA channel) and each can have different erosion settings applied to it. The lower layer represents the base rock layer and the top layer represents soft soil/sand were any sediment is deposited.

See [home page](https://www.digital-dust.com/single-post/2017/03/20/Interactive-erosion-in-Unity) for more information and unity package download.

![Erosion before](https://static.wixstatic.com/media/1e04d5_6721007a4e2c4e3c8c50d1162bfc6b21~mv2.jpg/v1/fill/w_550,h_255,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_6721007a4e2c4e3c8c50d1162bfc6b21~mv2.jpg)

![Erosion after](https://static.wixstatic.com/media/1e04d5_4660dbb922224cffad11afdf25d52511~mv2.jpg/v1/fill/w_550,h_254,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_4660dbb922224cffad11afdf25d52511~mv2.jpg)

![Slippage before](https://static.wixstatic.com/media/1e04d5_7f43e789451848328c0bb8387a98f611~mv2.jpg/v1/fill/w_550,h_254,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_7f43e789451848328c0bb8387a98f611~mv2.jpg)

![Slippage after](https://static.wixstatic.com/media/1e04d5_17f8774e4090442aaaa55526ac70a119~mv2.jpg/v1/fill/w_550,h_255,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_17f8774e4090442aaaa55526ac70a119~mv2.jpg)




