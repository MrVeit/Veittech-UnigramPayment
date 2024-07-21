# UNIGRAM PAYMENT

[![License](https://img.shields.io/github/license/MrVeit/Veittech-UnigramPayment?color=318CE7&style=flat-square)](LICENSE)
[![Version](https://img.shields.io/github/package-json/v/MrVeit/Veittech-UnigramPayment?color=318CE7&style=flat-square)](package.json)
[![Unity](https://img.shields.io/badge/Unity-2020.1+-2296F3.svg?color=318CE7&style=flat-square)](https://unity.com/releases/editor/archive)

**UNIGRAM PAYMENT** is a convenient solution for creating purchases in your Unity apps/games for the Telegram platform in the form of Telegram Stars. No need to connect payment providers as it was before.

# Technical Demo

You can test the SDK without installation in the TMA (Telegram Mini App) demo [via Telegram bot](https://t.me/UnigramPayment_bot/launch).

# Dependencies

Install the following plugins/libraries for the SDK to work correctly:

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

After installing and cloning the two repositories above, you can open the two projects in VS Code or any other code editor **that supports Node.js.**

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

- The `BOT_TOKEN` variable is the token of your Telegram bot, which can be created in [Bot Father](https://t.me/BotFather) inside Telegram by following the following simple steps. After creating the bot, you need to enter the command in **`/mybots`** and then go to the following path: `Select your bot -> Go to API Token -> Copy the value from there -> Paste the variable value without quotes`,

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
public sealed class UsageTemplate : MonoBehaviour
{
    private UnigramPaymentSDK _unigramPayment;

    private void OnDisable()
    {
        _unigramPayment.OnInitialized -= UnigramPaymentInitialized;
    }

    private void Start()
    {
        _unigramPayment = UnigramPaymentSDK.Instance;

        _unigramPayment.OnInitialized += UnigramPaymentInitialized;

        _unigramPayment.Initialize();
    }

    private void UnigramPaymentInitialized(bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("Success initialize Unigram Payment SDK");
        }
    }
}
```

#### Possible problems:

After writing a script to initialize the SDK. You may encounter a number of errors because the configuration of the connection to the test API server is not yet set up.

So you need to go to the configuration window via `Unigram Payment -> API Config`.
Now you need to fill the `Client Secret Key` field with the value you previously entered for the API server variable `CLIENT_SECRET_KEY`.
You can leave the `Server Url` field unchanged if you want to do local testing.

# Usage Template

Now it's time to look at examples of using the Unigram Payment `library API`.
After successful initialization, you can create a test invoice for payment.

**IMPORTANT:** the library makes a special storage with products in the form of Scriptable Object, the configuration of which contains such fields as: `Id`, `Name`, `Description` and its `Price` in Telegram Stars.

You can find this storage by going to `Assets -> Unigram Payment -> Items Storage`. To add your own items, right click on the project window and go to `Create -> Unigram Payment -> Saleable Item`.

### Creating a payment invoice

Below you can see an example of creating an invoice to pay for an item in Telegram Stars:

```c#
public sealed class UsageTemplate : MonoBehaviour
{
    [SerializeField, Space] private Button _createInvoiceButton;
    [SerializeField, Space] private SaleableItemsStorage _itemsStorage;

    private UnigramPaymentSDK _unigramPayment;

    private string _latestInvoice;

    private void OnDisable()
    {
        _createInvoiceButton.onClick.RemoveListener(CreateInvoice);

        _unigramPayment.OnInitialized -= UnigramPaymentInitialized;

        _unigramPayment.OnInvoiceLinkCreated -= PaymentInvoiceCreated;
        _unigramPayment.OnInvoiceLinkCreateFailed -= PaymentInvoiceCreateFailed;
    }

    private void Start()
    {
        _createInvoiceButton.onClick.AddListener(CreateInvoice);

        _unigramPayment = UnigramPaymentSDK.Instance;

        _unigramPayment.OnInitialized += UnigramPaymentInitialized;

        _unigramPayment.OnInvoiceLinkCreated += PaymentInvoiceCreated;
        _unigramPayment.OnInvoiceLinkCreateFailed += PaymentInvoiceCreateFailed;

        _unigramPayment.Initialize();
    }

    private void CreateInvoice()
    {
        var randomItemFromStorage = _itemsStorage.Items[Random.Range(0, _itemsStorage.Items.Count - 1)];

        Debug.Log($"Claimed item with payload id: {randomItemFromStorage.Id}");

        _unigramPayment.CreateInvoice(randomItemFromStorage);
    }

    private void UnigramPaymentInitialized(bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("Success initialize Unigram Payment SDK");
        }
    }

    private void PaymentInvoiceCreated(string invoiceLink)
    {
        _latestInvoice = invoiceLink;

        Debug.Log("The link to purchase the test item has been successfully generated: {url}");
    }

    private void PaymentInvoiceCreateFailed()
    {
        Debug.LogError("Failed to create a payment link for one of the following reasons");
    }
}
```

Now you will easily get a payment link, which you can open in your browser and pay in `your Telegram bot` if it was launched locally.

**IMPORTANT:** Processing a callback with receipt of payment check and subsequent refund **NOT AVAILABLE IN EDITOR**. So you need to create an assembly for WebGL and upload it to `Github Pages` or anywhere else where you have an `HTTPS Connection` and a valid `SSL Certificate` (I won't describe a detailed tutorial here, as you can find that online).

**P.S:** for detailed information on how to properly build a project with the Unigram Payment library, go to the [`Build`](https://github.com/MrVeit/Veittech-UnigramPayment#build) section.

### Invoice opening and payment

The following shows the implementation of opening and paying an invoice. The result is processed through appropriate callbacks from receipt of the check when payment is successful or unsuccessful:

```c#
public sealed class UsageTemplate : MonoBehaviour
{
    [SerializeField, Space] private Button _createInvoice;
    [SerializeField] private Button _openInvoice;
    [SerializeField, Space] private SaleableItemsStorage _itemsStorage;

    private UnigramPaymentSDK _unigramPayment;

    private PaymentReceiptData _itemPaymentReceipt;

    private string _latestInvoice;

    private void OnDisable()
    {
        _createInvoice.onClick.RemoveListener(CreateInvoice);
        _openInvoice.onClick.RemoveListener(OpenInvoice);

        _unigramPayment.OnInitialized -= UnigramPaymentInitialized;

        _unigramPayment.OnInvoiceLinkCreated -= PaymentInvoiceCreated;
        _unigramPayment.OnInvoiceLinkCreateFailed -= PaymentInvoiceCreateFailed;

        _unigramPayment.OnItemPurchased -= ItemPurchased;
        _unigramPayment.OnItemPurchaseFailed -= ItemPurchaseFailed;
    }

    private void Start()
    {
        _createInvoice.onClick.AddListener(CreateInvoice);
        _openInvoice.onClick.AddListener(OpenInvoice);

        _unigramPayment = UnigramPaymentSDK.Instance;

        _unigramPayment.OnInitialized += UnigramPaymentInitialized;

        _unigramPayment.OnInvoiceLinkCreated += PaymentInvoiceCreated;
        _unigramPayment.OnInvoiceLinkCreateFailed += PaymentInvoiceCreateFailed;

        _unigramPayment.OnItemPurchased += ItemPurchased;
        _unigramPayment.OnItemPurchaseFailed += ItemPurchaseFailed;

        _unigramPayment.Initialize();
    }

    private void CreateInvoice()
    {
        var randomItemFromStorage = _itemsStorage.Items[Random.Range(0, _itemsStorage.Items.Count - 1)];

        Debug.Log($"Claimed item with payload id: {randomItemFromStorage.Id}");

        _unigramPayment.CreateInvoice(randomItemFromStorage);
    }
        
    private void OpenInvoice()
    {
        _unigramPayment.OpenInvoice(_latestInvoice);
    }

    private void UnigramPaymentInitialized(bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("Success initialize Unigram Payment SDK");
        }
    }

    private void PaymentInvoiceCreated(string invoiceLink)
    {
        _latestInvoice = invoiceLink;

        Debug.Log($"The link to purchase the test item has been successfully generated: {url}");
    }

    private void PaymentInvoiceCreateFailed()
    {
        Debug.LogError("Failed to create a payment link for one of the following reasons");
    }

    private void ItemPurchased(PaymentReceiptData receipt)
    {
        _itemPaymentReceipt = receipt;

        Debug.Log($"The item with identifier {_itemPaymentReceipt.InvoicePayload} " +
                $"was successfully purchased for {_itemPaymentReceipt.Amount} " +
                $"stars by the buyer with telegram id {_itemPaymentReceipt.BuyerId}");
    }

    private void ItemPurchaseFailed()
    {
        Debug.LogError("Failed to purchase an item for one of the following reasons");
    }
}
```

When called to open a **previously generated invoice**, you will be presented with a native Pop up window to make a payment, which you can close without payment or pay - the results of both cases will be processed by the SDK.

### Payment refund

The following shows the implementation of a call to return a previously paid invoice:

```c#
public sealed class UsageTemplate : MonoBehaviour
{
    [SerializeField, Space] private Button _createInvoice;
    [SerializeField] private Button _openInvoice;
    [SerializeField] private Button _refundPayment;
    [SerializeField, Space] private SaleableItemsStorage _itemsStorage;

    private UnigramPaymentSDK _unigramPayment;

    private PaymentReceiptData _itemPaymentReceipt;

    private string _latestInvoice;

    private void OnDisable()
    {
        _createInvoice.onClick.RemoveListener(CreateInvoice);
        _openInvoice.onClick.RemoveListener(OpenInvoice);
        _refundPayment.onClick.RemoveListener(Refund);

        _unigramPayment.OnInitialized -= UnigramPaymentInitialized;

        _unigramPayment.OnInvoiceLinkCreated -= PaymentInvoiceCreated;
        _unigramPayment.OnInvoiceLinkCreateFailed -= PaymentInvoiceCreateFailed;

        _unigramPayment.OnItemPurchased -= ItemPurchased;
        _unigramPayment.OnItemPurchaseFailed -= ItemPurchaseFailed;

        _unigramPayment.OnRefundTransactionFinished -= RefundTransactionFinished;
    }

    private void Start()
    {
        _createInvoice.onClick.AddListener(CreateInvoice);
        _openInvoice.onClick.AddListener(OpenInvoice);
        _refundPayment.onClick.AddListener(Refund);

        _unigramPayment = UnigramPaymentSDK.Instance;

        _unigramPayment.OnInitialized += UnigramPaymentInitialized;

        _unigramPayment.OnInvoiceLinkCreated += PaymentInvoiceCreated;
        _unigramPayment.OnInvoiceLinkCreateFailed += PaymentInvoiceCreateFailed;

        _unigramPayment.OnItemPurchased += ItemPurchased;
        _unigramPayment.OnItemPurchaseFailed += ItemPurchaseFailed;

        _unigramPayment.OnRefundTransactionFinished += RefundTransactionFinished;

        _unigramPayment.Initialize();
    }

    private void CreateInvoice()
    {
        var randomItemFromStorage = _itemsStorage.Items[Random.Range(0, _itemsStorage.Items.Count - 1)];

        Debug.Log($"Claimed item with payload id: {randomItemFromStorage.Id}");

        _unigramPayment.CreateInvoice(randomItemFromStorage);
    }
        
    private void OpenInvoice()
    {
        _unigramPayment.OpenInvoice(_latestInvoice);
    }

    private void Refund()
    {
        _unigramPayment.Refund(_itemPaymentReceipt);
    }

    private void UnigramPaymentInitialized(bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("Success initialize Unigram Payment SDK");
        }
    }

    private void PaymentInvoiceCreated(string invoiceLink)
    {
        _latestInvoice = invoiceLink;

        Debug.Log($"The link to purchase the test item has been successfully generated: {url}");
    }

    private void PaymentInvoiceCreateFailed()
    {
        Debug.LogError("Failed to create a payment link for one of the following reasons");
    }

    private void ItemPurchased(PaymentReceiptData receipt)
    {
        _itemPaymentReceipt = receipt;

        Debug.Log($"The item with identifier {_itemPaymentReceipt.InvoicePayload} " +
                $"was successfully purchased for {_itemPaymentReceipt.Amount} " +
                $"stars by the buyer with telegram id {_itemPaymentReceipt.BuyerId}");
    }

    private void ItemPurchaseFailed()
    {
        Debug.LogError("Failed to purchase an item for one of the following reasons");
    }

    private void RefundTransactionFinished(bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("The process of refunding the purchased stars through the transaction with" +
                    $" the identifier `{_unigramPayment.LastRefundedTransaction}` " +
                    $"has been completed successfully");
        }
    }
}
```

After you request a payment refund, the API server contacts the Telegram API for the `specified transaction id` and `buyer id`. The next step is to check if this payment from this user has been in your Telegram bot at all or if it has been previously refunded. After receiving the result, you can display some notification to the user about successful or unsuccessful refund.

### Access token update

The API server access token has its own expiration date, which you can change at your discretion in the `session.js` script on the server **(by default it is valid for an hour)**. Once this expires, access to the API for your Unity client is closed and you need to update it. The SDK provides **an automatic token update** if a failed request to the server was made with a corresponding **Unauthorized client, access denied** error. If you want to manually refresh the access token, then call the `UnigramPaymentSDK.Instance.RefreshToken()` method and subscribe to the successful refresh result `UnigramPaymentSDK.Instance.OnSessionTokenRefreshed`.

# Build

Before you start building your unity project in WebGl, you need to do a few things to get the library **working properly.**

Go to the `Build Settings` window, then open `Project Settings -> Player -> Resolution and Presentation` and select the `Unigram Pay` build template. To display correctly in Telegram Web View, you need to set `Default Canvas Width` to 1080 and `Default Canvas Height` to 1920, and disable the `Run in Background` option.

These are all the necessary steps that need to be done for the project **to build successfully** and for the library functions **to work properly.**

# Production Deploy

# Donations

If you want to support my work you can send Toncoins to this address:
```
UQDPwEk-cnQXEfFaaNVXywpbKACUMwVRupkgWjhr_f4Ursw6
```

**Thanks for your support!**

# Support

[![Email](https://img.shields.io/badge/-gmail-090909?style=for-the-badge&logo=gmail)](https://mail.google.com/mail/?view=cm&fs=1&to=misster.veit@gmail.com)
[![Telegram](https://img.shields.io/badge/-Telegram-090909?style=for-the-badge&logo=telegram)](https://t.me/unigram_tools)
