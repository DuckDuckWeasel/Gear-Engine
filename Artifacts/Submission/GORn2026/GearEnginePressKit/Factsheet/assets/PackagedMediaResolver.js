(() => {
  const mediaMap = {"file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentRaceActionFigureEight.png": "./assets/media/T_CurrentRaceActionFigureEight.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentRewardSelection.png": "./assets/media/T_CurrentRewardSelection.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentStorage.png": "./assets/media/T_CurrentStorage.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Assets/GearEngine/Art/Splash%20Screen/Game%20Icon.png": "./assets/media/GameIcon.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/BrandExploration/GearEngineRouteMark512.png": "./assets/media/GearEngineRouteMark512.png"};
  const resolveMedia = (value) => {
    if (!value) return value;
    if (mediaMap[value]) return mediaMap[value];
    try {
      const decoded = decodeURI(value);
      return mediaMap[decoded] || value;
    } catch (_error) {
      return value;
    }
  };
  const rewriteNode = (root) => {
    if (!root || !root.querySelectorAll) return;
    root.querySelectorAll("img[src],source[src],video[poster]").forEach((element) => {
      const attribute = element.hasAttribute("poster") ? "poster" : "src";
      const current = element.getAttribute(attribute);
      const resolved = resolveMedia(current);
      if (resolved && resolved !== current) element.setAttribute(attribute, resolved);
    });
  };
  window.addEventListener("load", () => {
    [100, 500, 1500].forEach((delay) => setTimeout(() => rewriteNode(document), delay));
  }, { once: true });
})();
