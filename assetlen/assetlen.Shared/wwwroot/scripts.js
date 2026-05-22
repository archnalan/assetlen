window.initializeHoverListeners = (dotNetRef) => {
  const buttons = [
    { id: "button1", popoverId: "button1" },
    { id: "button2", popoverId: "button2" },
  ];

  buttons.forEach(({ id, popoverId }) => {
    const button = document.getElementById(id);

    if (button) {
      button.addEventListener("mouseenter", () => {
        dotNetRef.invokeMethodAsync("SetPopoverState", popoverId, true);
      });

      button.addEventListener("mouseleave", () => {
        dotNetRef.invokeMethodAsync("SetPopoverState", popoverId, false);
      });
    }
  });
};
window.clearSorting = (element) => {
  element.sortDefinitions = [];
};

window.setupBeforeUnloadListener = (dotNetRef) => {
  window.addEventListener("beforeunload", async (e) => {
    const hasChanges = await dotNetRef.invokeMethodAsync("HasUnsavedChanges");
    if (hasChanges) {
      e.preventDefault();
      e.returnValue = "";
    }
  });
};

window.assetlen = window.assetlen || {};
window.assetlen.lightbox = (() => {
  let ref = null;
  let prevOverflow = null;
  const onKey = (e) => {
    if (!ref) return;
    if (e.key === "Escape" || e.key === "ArrowLeft" || e.key === "ArrowRight") {
      e.preventDefault();
      ref.invokeMethodAsync("OnKey", e.key);
    }
  };
  return {
    attach(dotNetRef) {
      ref = dotNetRef;
      prevOverflow = document.body.style.overflow;
      document.body.style.overflow = "hidden";
      document.addEventListener("keydown", onKey);
    },
    detach() {
      document.removeEventListener("keydown", onKey);
      document.body.style.overflow = prevOverflow ?? "";
      prevOverflow = null;
      ref = null;
    },
  };
})();
