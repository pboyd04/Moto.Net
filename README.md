# Moto.Net
A C# client to handle MotoTrbo including native wire line communication, LRRP, and TMS

# MotoMond
This service logs radios and calls. It also supports the following commands:

1. check <radio id> - Issues a radio check to the specified radio via the IP connected repeater
2. locate <radio id> - Sends an LRRP request to the specified radio via a USB connected control radio
3. test <radio id> <message> - Sends a text message to the specified radio via a USB connected control radio
4. getip <radio id> - Gets the IP address of the specified radio

# Why did I write this?
I had a few use cases in mind. Some of which I've acomplished others I am still working on.
Working use cases:
1. I wanted to be able to determine, objetively, if changes I made to the repeater setup were improving reception or not. To do this I needed to know where the radio was (LRRP) and the strength of the signal (IPSC) at that location. Combining the two I was able to graph out how my improvements were helping.
2. I wanted to know how many talkgroups in my Capacity Plus setup were in use at once. So that I can determine correct capacity.
3. I wanted to play around with interfacing the radio system with internet audio like Discord (Bot will be uploaded soon), Mumble (still have some issues here)m and Zello (client also soon). 
4. I wanted to get alerted and log repeater errors somewhere more useful than RDAC seems capable of.
Not yet working cases:
1. I wanted to be able to quickly inventory my radios on the air including ID (working), Serial Number, and Firmware Version. 

# How did you write this? Did you hack other sources?
I used some online sources such as [DMRLink](https://github.com/adamfast/DMRlink) and a [Wireshark dissector](https://github.com/george-hopkins/xcmp-xnl-dissector). I then started out rewriting it in [Golang](https://github.com/pboyd04/MotoGo). And while I do like Go I ran into a few issues without inheritence.

But I digress, from there I used packet captures and some source I found elsewhere online (I don't recall where at the moment) to determine the handshake for XNL to repeaters. This took a lot of cycles on my GPU to reverse engineer. 

# XNL Constants
This library allows for using some of Motorola's proprietary XNL/XCMP protocol. However, to actually use it requires some constants to manipulate a payload. Those constants aren't in the repo. Ideally this should be an exercise for you to figure out how to derive them. However, I have added code that will detect if the constants aren't defined and will then take other software you may have (TRBONet) and use its built in decrypting library instead. Just drop the TRBONet.Server.exe file from an install into the same folder as the executable running Moto.Net and it will figure it out from there.

# Audio decoding
This library allows for audio decoding, however it requires an external vocoder library to decode the AMBE+ codec. That too you have to obtain on your own. If someone wants to write a layer to use an open version that is fine, but I have no desire to include the vocoder in this code.
