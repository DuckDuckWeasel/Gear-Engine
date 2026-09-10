(() => {
  const mediaMap = {"file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/VisualTests/GearWorkspaceScreenSpace/Baseline.png": "./assets/media/Baseline.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Assets/GearEngine/Art/Splash%20Screen/Game%20Icon.png": "./assets/media/GameIcon.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Assets/GearEngine/Art/UI/Cog%20Runner%20Screen%201%20Ref.png": "./assets/media/CogRunnerScreen1Ref.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/ContactSheets/EvidenceContactSheet01.png": "./assets/media/EvidenceContactSheet01.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/VideoEvolution/T_VideoEvolutionTimeline.png": "./assets/media/T_VideoEvolutionTimeline.png", "file:///Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/VideoEvolution/T_LatestBuildAiReferenceSheet.png": "./assets/media/T_LatestBuildAiReferenceSheet.png"};
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
