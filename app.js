var appInstance;
var unsubscribeEvent;

var container = document.querySelector("#unity-container");
var canvas = document.querySelector("#unity-canvas");
var loadingBar = document.querySelector("#unity-loading-bar");
var progressBarFull = document.querySelector("#unity-progress-bar-full");

var buildUrl = "Build";
var loaderUrl = buildUrl + "/Veittech-UnigramPayment-WebPage.loader.js";
var config = {
  dataUrl: buildUrl + "/Veittech-UnigramPayment-WebPage.data.unityweb",
  frameworkUrl: buildUrl + "/Veittech-UnigramPayment-WebPage.framework.js.unityweb",
  codeUrl: buildUrl + "/Veittech-UnigramPayment-WebPage.wasm.unityweb",
  streamingAssetsUrl: "StreamingAssets",
  companyName: "Veittech",
  productName: "Unigram Payment",
  productVersion: "1.0.8"
};

if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent))
{
  var meta = document.createElement('meta');
    
  meta.name = 'viewport';
  meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
    
  document.getElementsByTagName('head')[0].appendChild(meta);
}
  
loadingBar.style.display = "block";
  
var script = document.createElement("script");
script.src = loaderUrl;
  
script.onload = () => 
{
    createUnityInstance(canvas, config, (progress) => 
    {
      progressBarFull.style.width = 100 * progress + "%";
    }
    ).then((unityInstance) => 
    {
      appInstance = unityInstance;
      
      loadingBar.style.display = "none";
    }
    ).catch((message) => 
    {
      alert(message);
    });
};
  
document.body.appendChild(script);
  
window.addEventListener('load', function ()
{
  Telegram.WebApp.ready();
  Telegram.WebApp.expand();
  
  console.log("Telegram Web App has been expanded to full screen");
  
  var version = Telegram.WebApp.version;
  var versionFloat = parseFloat(version);
  
  if (versionFloat >= 7.7)
  {
      Telegram.WebApp.disableVerticalSwipes();
          
      console.log('Activating vertical swipe disable');
  }
  
  console.log(`Telegram Web App opened with version: ${version}`);
  console.log(`Telegram Web App checked latest version status with `+
      `result: ${Telegram.WebApp.isVersionAtLeast(version)}`);
});
