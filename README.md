# Moto.Net
A C# client to handle MotoTrbo including native wire line communication, LRRP, and TMS

# MotoMond
This service logs radios and calls. It also supports the following commands:

1. check <radio id> - Issues a radio check to the specified radio via the IP connected repeater
2. locate <radio id> - Sends an LRRP request to the specified radio via a USB connected control radio
3. test <radio id> <message> - Sends a text message to the specified radio via a USB connected control radio
4. getip <radio id> - Gets the IP address of the specified radio

# XNL Constants
This library allows for using some of Motorola's proprietary XNL/XCMP protocol. However, to actually use it requires some constants to manipulate a payload. Those constants aren't in the repo. I will leave it up to you to figure out how to derive them.

# Audio decoding
This library allows for audio decoding, however it requires an external vocoder library to decode the AMBE+ codec. That too you have to obtain on your own. If someone wants to write a layer to use an open version that is fine, but I have no desire to include the vocoder in this code.
