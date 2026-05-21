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
