mergeInto(LibraryManager.library, {
  GearEngineLoadingReady: function () {
    if (typeof window.gearEngineLoadingReady === "function") {
      window.gearEngineLoadingReady();
    }
  },
});
