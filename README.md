# UNIGRAM PAYMENT

[![License](https://img.shields.io/github/license/MrVeit/Veittech-UnigramPayment?color=318CE7&style=flat-square)](LICENSE)
[![Version](https://img.shields.io/github/package-json/v/MrVeit/Veittech-UnigramPayment?color=318CE7&style=flat-square)](package.json)
[![Unity](https://img.shields.io/badge/Unity-2020.1+-2296F3.svg?color=318CE7&style=flat-square)](https://unity.com/releases/editor/archive)

**UNIGRAM PAYMENT** is a convenient solution for creating purchases in your Unity apps/games for the Telegram platform in the form of Telegram Stars. No need to connect payment providers as it was before.

# Technical Demo

You can test the SDK without installation on the demo app [in Telegram bot](https://t.me/UnigramPayment_bot/launch).

# Dependencies

For the library to work correctly, the following dependencies **must be installed** in the project before use:
- **[Newtonsoft](https://www.youtube.com/watch?v=3H6xkl_EsvQ)** - modern solution for convenient work with json files.

# Installation

[Download the latest version of the SDK via the .unityPackage file here](https://github.com/MrVeit/Veittech-UnigramPayment/releases).

# Initialization

### Initializing backend components

Before you start testing the library in your project, you need to set up the base, without which it will not work.
To do this, it is necessary to run locally Server API and Telegram Bot, in which the logic of payments will be conducte.

To do this, you need to make a clone of the Server API repository that is written in Node.js:
```
https://github.com/MrVeit/Veittech-UnigramPayment-ServerAPI
```
And also a clone of the Telegram Bot repository, which is also written in Node.js:
```
https://github.com/MrVeit/Veittech-UnigramPayment-TelegramBot
```

If you already have Node.js installed on your Windows computer (does anyone make Unity games on Linux? :D), you can skip this step and move on to the next one. If you still don't have it installed, you need to go to the official Node.js website and [install it yourself](https://nodejs.org/en/).

After installing and cloning the two repositories above, you can open the two projects in VS Code or any other code editor that supports Node.js.

Now, in order to be able to run these projects locally and start testing, you need to create an environment variable repository. To do this, you need to create a file named `.env` but **without any file format** in the directory of both repositories.

For the API Server environment variable repository, you need to fill in the following information:

```config
SERVER_DOMAIN = http://localhost

BOT_TOKEN = YOUR_BOT_TOKEN
BOT_SECRET_KEY = “test_app_unigram_payment”

CLIENT_SECRET_KEY = “test_unity_unigram”
CLIENT_JWT_SIGN = “unigram_payment-unity”
```

- The `SERVER_DOMAIN` variable is a link to your domain where the API server is running, for the purposes of testing we will use the local address `http://localhost`,

- The `BOT_TOKEN` variable is your Telegram bot token, which can be created in [Bot Father](https://t.me/BotFather) inside Telegram by following simple steps. After creating the bot, you need to enter the command in [Bot Father](https://t.me/BotFather) `/mybots -> Select your bot -> Go to API Token -> Copy the value from there -> Paste the variable value without quotes`,

- The `BOT_SECRET_KEY` variable is the signature key by which the API server will identify your bot when it receives a check for payment. Ignoring third-party requests if they do not match this value, after decrypting the bot token.
Here you can use a pair of two words or one with numbers, using any special characters.
**IMPORTANT:** Store this key securely, it should not be public,

- The `CLIENT_SECRET_KEY` variable is the same signature key as the previous one, but for your Unity game. With it, before creating a payment request, you will need to authorize on the API server. ** IMPORTANT:** It must also be stored securely and must not be public,

- The `CLIENT_JWT_SIGN` variable is an additional signing key with which your Unity game, after authorizing to the API server and receiving a generated JWT token, is signed with this key. Then, when requesting other API methods, the Unity game sends this generated JWT token in the Authorizatio header, and the server decrypts the token value with this key to allow access to its functionality if the values match.

For the Telegram bot, the environment variables will look like this:
```config
BOT_TOKEN = YOUR_BOT_TOKEN
SERVER_DOMAIN = http://localhost:1000

AUTHORIZATION_SECRET_KEY = “test_app_unigram_payment”
```

- The `BOT_TOKEN` variable should contain the bot token that you have previously filled in and inserted into a variable of the same name for the server API.
- The `SERVER_DOMAIN` variable is a reference to your API server. For testing purposes, leave the specified value unchanged.
- The `AUTHORIZATION_SECRET_KEY` variable should contain the same value that you filled in for `BOT_SECRET_KEY` for the API server.

Now you can run both projects to start testing. 
Open the terminal in the code editor in which you opened these projects and enter the following commands:

To activate the API server, enter the command:
```
node server.js
```

To activate the Telegram bot, enter the command:
```
npm start
```

**IMPORTANT:** In case you encounter a startup problem at this stage, it means you don't have Node.js installed or it was installed incorrectly.
Try reinstalling or searching for a solution to your problem on the Internet.

### Initializing the Unity Client

Once the necessary backend components have been successfully installed and running, you can start customizing your Unity project.

#### Automatic Initialization:
The `UnigramPaymentSDK` component has an option `Initialize On Awake`. When it is activated, the SDK is initialized automatically. You will only have to subscribe to the necessary events and start working with it.

#### Manual Initialization:
Below is a test example of what this might look like.

```c#

```

# Donations

If you want to support my work you can send Toncoins to this address:
```
UQDPwEk-cnQXEfFaaNVXywpbKACUMwVRupkgWjhr_f4Ursw6
```

**Thanks for your support!**

# Support

[![Email](https://img.shields.io/badge/-gmail-090909?style=for-the-badge&logo=gmail)](https://mail.google.com/mail/?view=cm&fs=1&to=misster.veit@gmail.com)
[![Telegram](https://img.shields.io/badge/-Telegram-090909?style=for-the-badge&logo=telegram)](https://t.me/MrVeit)
