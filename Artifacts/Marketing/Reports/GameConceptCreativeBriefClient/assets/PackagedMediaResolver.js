(() => {
  const mediaMap = {"file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/GearEngineRouteBoard.png": "./assets/media/GearEngineRouteBoard.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/BrandExploration/GearEngineRouteMark512.png": "./assets/media/GearEngineRouteMark512.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/CogRunnerRouteBoard.png": "./assets/media/CogRunnerRouteBoard.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/BrandExploration/CogRunnerRouteMark512.png": "./assets/media/CogRunnerRouteMark512.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/ClockworkApexRouteBoard.png": "./assets/media/ClockworkApexRouteBoard.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/BrandExploration/ClockworkApexRouteMark512.png": "./assets/media/ClockworkApexRouteMark512.png"};
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
