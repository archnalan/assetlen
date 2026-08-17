window.forwardEnterToTab = function () {
  let activeElement = document.activeElement;
  if (activeElement.tagName === "INPUT") {
    let formElements = Array.from(
      document.querySelectorAll("input, button, select, textarea"),
    );
    let currentIndex = formElements.indexOf(activeElement);
    if (currentIndex >= 0 && currentIndex < formElements.length - 1) {
      formElements[currentIndex + 1].focus();
    }
  }
};
window.getBoundingClientRect = function (element) {
  return element.getBoundingClientRect();
};
window.getElementHeight = (elementId) => {
  const element = document.getElementById(elementId);
  return element ? element.offsetHeight : 0;
};
window.generateReceiptPDF = async (elementId) => {
  try {
    const element = document.getElementById(elementId);
    const canvas = await html2canvas(element, {
      scale: 2,
      logging: false,
      useCORS: true,
    });

    const pdf = new jspdf.jsPDF({
      orientation: "portrait",
      unit: "mm",
      format: [canvas.width * 0.264583, canvas.height * 0.264583],
    });

    pdf.addImage(
      canvas,
      "PNG",
      0,
      0,
      pdf.internal.pageSize.getWidth(),
      pdf.internal.pageSize.getHeight(),
    );

    // Return as Base64 string instead of ArrayBuffer
    return pdf.output("datauristring");
  } catch (error) {
    console.error("PDF generation error:", error);
    return null;
  }
};

window.downloadFile = (filename, contentType, data) => {
  const blob = new Blob([data], { type: contentType });
  const link = document.createElement("a");
  link.href = window.URL.createObjectURL(blob);
  link.download = filename;
  link.click();
};

// BookReader.js - JavaScript interop for BookReader component

window.scrollToElement = function (elementId) {
  const element = document.getElementById(elementId);
  if (element) {
    element.scrollIntoView({ behavior: "smooth", block: "start" });
    return true;
  }
  return false;
};

window.getScrollPosition = function () {
  return {
    x: window.pageXOffset || document.documentElement.scrollLeft,
    y: window.pageYOffset || document.documentElement.scrollTop,
  };
};

window.setScrollPosition = function (x, y) {
  window.scrollTo(x, y);
};

window.saveScrollPosition = function (key) {
  const position = window.getScrollPosition();
  sessionStorage.setItem(key, JSON.stringify(position));
};

window.restoreScrollPosition = function (key) {
  const saved = sessionStorage.getItem(key);
  if (saved) {
    const position = JSON.parse(saved);
    window.setScrollPosition(position.x, position.y);
  }
};

window.getElementPosition = function (elementId) {
  const element = document.getElementById(elementId);
  if (element) {
    const rect = element.getBoundingClientRect();
    return {
      top: rect.top + window.pageYOffset,
      left: rect.left + window.pageXOffset,
      width: rect.width,
      height: rect.height,
    };
  }
  return null;
};

// Track reading time
window.startReadingTimer = function (dotNetHelper, productId) {
  if (window.readingTimer) {
    clearInterval(window.readingTimer);
  }

  let startTime = Date.now();
  let totalSeconds = 0;

  window.readingTimer = setInterval(function () {
    totalSeconds++;

    // Update every minute
    if (totalSeconds % 60 === 0) {
      dotNetHelper.invokeMethodAsync(
        "UpdateReadingTime",
        productId,
        Math.floor(totalSeconds / 60),
      );
    }
  }, 1000);
};

window.stopReadingTimer = function () {
  if (window.readingTimer) {
    clearInterval(window.readingTimer);
    window.readingTimer = null;
  }
};

// Intersection Observer for tracking visible sections
window.observeSections = function (dotNetHelper) {
  if (window.sectionObserver) {
    window.sectionObserver.disconnect();
  }

  const options = {
    root: null,
    rootMargin: "-20% 0px -70% 0px",
    threshold: 0,
  };

  window.sectionObserver = new IntersectionObserver(function (entries) {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        const sectionId = entry.target.id;
        const match = sectionId.match(/section-(\d+)/);
        if (match) {
          const sectionIndex = parseInt(match[1]);
          dotNetHelper.invokeMethodAsync("OnSectionVisible", sectionIndex);
        }
      }
    });
  }, options);

  // Observe all section elements
  document.querySelectorAll('[id^="section-"]').forEach((section) => {
    window.sectionObserver.observe(section);
  });
};

window.disconnectObserver = function () {
  if (window.sectionObserver) {
    window.sectionObserver.disconnect();
    window.sectionObserver = null;
  }
};

// Font size utilities
window.setFontSize = function (fontSize) {
  const readingContent = document.querySelector(".reading-content");
  if (readingContent) {
    readingContent.style.fontSize = fontSize + "px";
  }
};

window.setTheme = function (theme) {
  document.documentElement.setAttribute("data-theme", theme.toLowerCase());
};

// Full screen toggle
window.toggleFullScreen = function () {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen().catch((err) => {
      console.error("Error attempting to enable full-screen mode:", err);
    });
    return true;
  } else {
    if (document.exitFullscreen) {
      document.exitFullscreen();
    }
    return false;
  }
};

// Highlight text selection
window.highlightSelection = function (color) {
  const selection = window.getSelection();
  if (selection.rangeCount > 0) {
    const range = selection.getRangeAt(0);
    const span = document.createElement("span");
    span.style.backgroundColor = color;
    span.className = "user-highlight";

    try {
      range.surroundContents(span);
      return {
        text: selection.toString(),
        color: color,
        position: {
          start: range.startOffset,
          end: range.endOffset,
        },
      };
    } catch (e) {
      console.error("Error highlighting text:", e);
      return null;
    }
  }
  return null;
};

// Copy text to clipboard
window.copyToClipboard = function (text) {
  return navigator.clipboard
    .writeText(text)
    .then(() => {
      return true;
    })
    .catch((err) => {
      console.error("Failed to copy text:", err);
      return false;
    });
};

// Track page visibility for analytics
window.trackPageVisibility = function (dotNetHelper) {
  document.addEventListener("visibilitychange", function () {
    if (document.hidden) {
      dotNetHelper.invokeMethodAsync("OnPageHidden");
    } else {
      dotNetHelper.invokeMethodAsync("OnPageVisible");
    }
  });
};

// Clean up on page unload
window.addEventListener("beforeunload", function () {
  window.stopReadingTimer();
  window.disconnectObserver();
});

// DocumentEditor.js - Modern book editing experience with scroll sync and section tracking
(function () {
  // Flag to prevent observer from interfering during manual scrolls
  let isManualScrolling = false;
  let scrollTimeout = null;
  let observerEnabled = true;
  let activeObserver = null;

  // Debounce helper for scroll events
  function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  }

  window.initializeDocumentEditor = function (
    dotNetRef,
    containerId,
    sectionSelector,
  ) {
    const container = document.querySelector(containerId);
    if (!container) {
      console.error("Container not found:", containerId);
      return;
    }

    // Cleanup any existing observer
    if (activeObserver) {
      activeObserver.disconnect();
    }

    // Debounced callback to update active section
    const updateActiveSection = debounce((sectionId) => {
      if (observerEnabled && !isManualScrolling && sectionId) {
        dotNetRef.invokeMethodAsync("UpdateActiveSectionFromScroll", sectionId);
      }
    }, 100);

    // Set up IntersectionObserver to track visible sections
    const observer = new IntersectionObserver(
      (entries) => {
        // Skip observer updates during manual scrolling or when disabled
        if (isManualScrolling || !observerEnabled) {
          return;
        }

        // Find the section with the highest intersection ratio in the top half of viewport
        let topMostSection = null;
        let maxRatio = 0;

        entries.forEach((entry) => {
          if (entry.isIntersecting && entry.intersectionRatio > maxRatio) {
            const rect = entry.boundingClientRect;
            const containerRect = container.getBoundingClientRect();

            // Only consider sections in the top third of the viewport for better UX
            if (rect.top < containerRect.top + containerRect.height / 3) {
              maxRatio = entry.intersectionRatio;
              topMostSection = entry.target;
            }
          }
        });

        if (topMostSection) {
          const sectionId = topMostSection.getAttribute("data-section-id");
          updateActiveSection(sectionId);
        }
      },
      {
        root: container,
        threshold: [0, 0.1, 0.25, 0.5, 0.75, 1.0],
        rootMargin: "-10% 0px -50% 0px", // Top-third bias
      },
    );

    // Observe all section editors
    const sections = container.querySelectorAll(sectionSelector);
    sections.forEach((section) => observer.observe(section));

    // Store observer for cleanup and re-observation
    activeObserver = observer;
    window.documentEditorObserver = observer;
    window.documentEditorContainer = container;
    window.documentEditorSectionSelector = sectionSelector;

    return true;
  };

  window.scrollToSection = function (containerId, sectionId, smooth = true) {
    const container = document.querySelector(containerId);
    const section = container.querySelector(`[data-section-id="${sectionId}"]`);

    if (!container || !section) {
      console.error("Container or section not found:", containerId, sectionId);
      return false;
    }

    // Set flag to prevent observer interference during manual scroll
    isManualScrolling = true;

    // Clear any existing timeout
    if (scrollTimeout) {
      clearTimeout(scrollTimeout);
    }

    // Calculate the absolute position of the section relative to the container
    // This works even for off-screen elements
    const offset = 20; // pixels from top of container
    const sectionTop = section.offsetTop;
    const targetScrollTop = sectionTop - offset;

    // Use scrollTo for smooth/auto behavior
    container.scrollTo({
      top: targetScrollTop,
      behavior: smooth ? "smooth" : "auto",
    });

    // Add pulse highlight animation
    section.classList.add("pulse-highlight");
    setTimeout(() => {
      section.classList.remove("pulse-highlight");
    }, 1500);

    // Re-enable observer after scroll animation completes
    // Smooth scroll typically takes 300-500ms, so we wait longer for safety
    scrollTimeout = setTimeout(
      () => {
        isManualScrolling = false;
        scrollTimeout = null;
      },
      smooth ? 800 : 100,
    );

    return true;
  };

  window.setObserverEnabled = function (enabled) {
    observerEnabled = enabled;
  };

  window.reobserveSections = function () {
    if (
      !window.documentEditorObserver ||
      !window.documentEditorContainer ||
      !window.documentEditorSectionSelector
    ) {
      console.warn("Observer not initialized, cannot reobserve");
      return;
    }

    const container = window.documentEditorContainer;
    const selector = window.documentEditorSectionSelector;
    const observer = window.documentEditorObserver;

    // Disconnect and reconnect to pick up new/changed sections
    observer.disconnect();

    const sections = container.querySelectorAll(selector);
    sections.forEach((section) => observer.observe(section));
  };

  window.cleanupObserver = function () {
    // Clear any pending scroll timeout
    if (scrollTimeout) {
      clearTimeout(scrollTimeout);
      scrollTimeout = null;
    }
    isManualScrolling = false;
    observerEnabled = true;

    if (window.documentEditorObserver) {
      window.documentEditorObserver.disconnect();
      window.documentEditorObserver = null;
    }

    if (activeObserver) {
      activeObserver.disconnect();
      activeObserver = null;
    }

    window.documentEditorContainer = null;
    window.documentEditorSectionSelector = null;
  };

  window.getWordCount = function (htmlContent) {
    if (!htmlContent) return 0;

    // Create a temporary element to strip HTML tags
    const temp = document.createElement("div");
    temp.innerHTML = htmlContent;
    const text = temp.textContent || temp.innerText || "";

    // Count words (split by whitespace and filter empty strings)
    const words = text
      .trim()
      .split(/\s+/)
      .filter((word) => word.length > 0);
    return words.length;
  };

  window.focusSection = function (sectionId) {
    const section = document.querySelector(`[data-section-id="${sectionId}"]`);
    if (section) {
      section.scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
  };

  window.getBoundingRect = function (selector) {
    const element = document.querySelector(selector);
    if (!element) {
      return { top: 0, height: 0, left: 0, width: 0 };
    }

    const rect = element.getBoundingClientRect();
    return {
      top: rect.top,
      height: rect.height,
      left: rect.left,
      width: rect.width,
    };
  };
})();

window.setupBeforeUnloadListener = (dotNetRef) => {
  window.addEventListener("beforeunload", async (e) => {
    const hasChanges = await dotNetRef.invokeMethodAsync("HasUnsavedChanges");
    if (hasChanges) {
      e.preventDefault();
      e.returnValue = "";
    }
  });
};
// ========================================
// Fragment Feedback System
// ========================================

(function () {
  let fragmentObserver = null;
  let activeFragmentId = null;
  let dotNetRefFeedback = null;

  /**
   * Initialize fragment selection for feedback
   */
  window.initializeFragmentFeedback = function (dotNetRef, containerSelector) {
    dotNetRefFeedback = dotNetRef;

    // Try to find container immediately, or wait for it
    let attempts = 0;
    const maxAttempts = 20;

    function tryInitialize() {
      const container = document.querySelector(containerSelector);
      if (!container) {
        attempts++;
        if (attempts < maxAttempts) {
          setTimeout(tryInitialize, 250);
          return;
        }
        console.warn(
          "Fragment feedback container not found after retries:",
          containerSelector,
        );
        return;
      }

      // Add click handlers to fragments
      setupFragmentClickHandlers(container);

      // Setup intersection observer for scroll-follow behavior
      setupFragmentObserver(container);

      // Add context menu handler
      setupContextMenu(container);
    }

    tryInitialize();
  };

  /**
   * Cleanup fragment feedback handlers
   */
  window.cleanupFragmentFeedback = function () {
    if (fragmentObserver) {
      fragmentObserver.disconnect();
      fragmentObserver = null;
    }
    dotNetRefFeedback = null;
    activeFragmentId = null;

    // Remove context menu
    const contextMenu = document.getElementById("fragment-context-menu");
    if (contextMenu) {
      contextMenu.remove();
    }
  };

  /**
   * Add unique fragment IDs to HTML content
   */
  window.addFragmentIds = function (htmlContent) {
    const parser = new DOMParser();
    const doc = parser.parseFromString(htmlContent || "<p></p>", "text/html");

    // Tags that should receive fragment IDs
    const fragmentTags = [
      "p",
      "h1",
      "h2",
      "h3",
      "h4",
      "h5",
      "h6",
      "blockquote",
      "li",
      "pre",
      "table",
      "img",
      "figure",
    ];

    fragmentTags.forEach((tag) => {
      doc.querySelectorAll(tag).forEach((element) => {
        if (!element.getAttribute("data-fragment-id")) {
          element.setAttribute("data-fragment-id", generateUUID());
        }
      });
    });

    return doc.body.innerHTML;
  };

  /**
   * Get fragment content by ID
   */
  window.getFragmentContent = function (containerSelector, fragmentId) {
    const container = document.querySelector(containerSelector);
    if (!container) return null;

    const fragment = container.querySelector(
      `[data-fragment-id="${fragmentId}"]`,
    );
    if (!fragment) return null;

    return {
      id: fragmentId,
      tagName: fragment.tagName.toLowerCase(),
      textContent: fragment.textContent,
      innerHTML: fragment.innerHTML,
      outerHTML: fragment.outerHTML,
    };
  };

  /**
   * Highlight a specific fragment
   */
  window.highlightFragment = function (
    containerSelector,
    fragmentId,
    highlight = true,
    autoFadeMs = 3000,
  ) {
    const container = document.querySelector(containerSelector);
    if (!container) return;

    // Remove existing highlights
    container.querySelectorAll(".fragment-highlighted").forEach((el) => {
      el.classList.remove("fragment-highlighted");
      el.classList.remove("fragment-highlight-fadeout");
    });

    if (highlight && fragmentId) {
      const fragment = container.querySelector(
        `[data-fragment-id="${fragmentId}"]`,
      );
      if (fragment) {
        fragment.classList.add("fragment-highlighted");
        fragment.scrollIntoView({ behavior: "smooth", block: "center" });

        // Auto-fade after specified time
        if (autoFadeMs > 0) {
          setTimeout(() => {
            fragment.classList.add("fragment-highlight-fadeout");
            // Remove classes after fade animation completes
            setTimeout(() => {
              fragment.classList.remove("fragment-highlighted");
              fragment.classList.remove("fragment-highlight-fadeout");
            }, 500); // 500ms for fade animation
          }, autoFadeMs);
        }
      }
    }
  };

  /**
   * Update feedback indicator on a specific fragment
   */
  window.updateFragmentFeedbackIndicator = function (fragmentId, hasFeedback) {
    const fragment = document.querySelector(
      `[data-fragment-id="${fragmentId}"]`,
    );
    if (!fragment) return;

    // Remove existing indicator
    const existingIndicator = fragment.querySelector(".feedback-indicator");
    if (existingIndicator) {
      existingIndicator.remove();
    }

    if (hasFeedback) {
      const indicator = document.createElement("span");
      indicator.className = "feedback-indicator";
      indicator.innerHTML = '<span class="feedback-count">•</span>';
      indicator.title = "Has feedback";
      indicator.onclick = (e) => {
        e.stopPropagation();
        if (dotNetRefFeedback) {
          dotNetRefFeedback.invokeMethodAsync(
            "OnFeedbackIndicatorClick",
            fragmentId,
          );
        }
      };

      fragment.style.position = "relative";
      fragment.appendChild(indicator);
    }
  };

  /**
   * Get all fragments with feedback indicators
   */
  window.getFragmentsInViewport = function (containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return [];

    const fragments = container.querySelectorAll("[data-fragment-id]");
    const visibleFragments = [];

    fragments.forEach((fragment) => {
      const rect = fragment.getBoundingClientRect();
      const isVisible = rect.top < window.innerHeight && rect.bottom > 0;

      if (isVisible) {
        visibleFragments.push({
          id: fragment.getAttribute("data-fragment-id"),
          top: rect.top,
          bottom: rect.bottom,
        });
      }
    });

    return visibleFragments;
  };

  /**
   * Show feedback indicators on fragments
   */
  window.showFeedbackIndicators = function (
    containerSelector,
    fragmentFeedbackMap,
  ) {
    const container = document.querySelector(containerSelector);
    if (!container) return;

    // Remove existing indicators
    container
      .querySelectorAll(".feedback-indicator")
      .forEach((el) => el.remove());

    // Add new indicators
    Object.keys(fragmentFeedbackMap).forEach((fragmentId) => {
      const fragment = container.querySelector(
        `[data-fragment-id="${fragmentId}"]`,
      );
      if (fragment) {
        const count = fragmentFeedbackMap[fragmentId];
        const indicator = document.createElement("span");
        indicator.className = "feedback-indicator";
        indicator.innerHTML = `<span class="feedback-count">${count}</span>`;
        indicator.title = `${count} comment${count > 1 ? "s" : ""}`;
        indicator.onclick = (e) => {
          e.stopPropagation();
          if (dotNetRefFeedback) {
            dotNetRefFeedback.invokeMethodAsync(
              "OnFeedbackIndicatorClick",
              fragmentId,
            );
          }
        };

        // Position relative to fragment
        fragment.style.position = "relative";
        fragment.appendChild(indicator);
      }
    });
  };

  // Private helper functions

  function setupFragmentClickHandlers(container) {
    container.addEventListener("click", function (e) {
      const fragment = e.target.closest("[data-fragment-id]");
      if (fragment && dotNetRefFeedback) {
        const fragmentId = fragment.getAttribute("data-fragment-id");
        // Don't trigger if clicking on feedback indicator
        if (!e.target.closest(".feedback-indicator")) {
          activeFragmentId = fragmentId;
        }
      }
    });
  }

  function setupContextMenu(container) {
    // First, ensure all content elements have fragment IDs
    ensureFragmentIds(container);

    // Create context menu element
    let contextMenu = document.getElementById("fragment-context-menu");
    if (!contextMenu) {
      contextMenu = document.createElement("div");
      contextMenu.id = "fragment-context-menu";
      contextMenu.className = "fragment-context-menu";
      contextMenu.innerHTML = `
                <button class="context-menu-item" data-action="rate">
                    <span class="context-icon">⭐</span> Rate this section
                </button>
                <button class="context-menu-item" data-action="comment">
                    <span class="context-icon">💬</span> Add comment
                </button>
                <button class="context-menu-item" data-action="suggest">
                    <span class="context-icon">✏️</span> Suggest edit
                </button>
            `;
      document.body.appendChild(contextMenu);

      // Handle menu item clicks
      contextMenu.addEventListener("click", function (e) {
        const button = e.target.closest(".context-menu-item");
        if (button && activeFragmentId && dotNetRefFeedback) {
          const action = button.getAttribute("data-action");
          const fragmentContent = window.getFragmentContent(
            container.classList[0]
              ? "." + container.classList[0]
              : "#" + container.id || ".reading-content",
            activeFragmentId,
          );

          dotNetRefFeedback.invokeMethodAsync(
            "OnFragmentAction",
            action,
            activeFragmentId,
            fragmentContent?.textContent || "",
          );
          hideContextMenu();
        }
      });
    }

    // Right-click handler - intercept on the entire container
    container.addEventListener("contextmenu", function (e) {
      // Find the nearest fragmentable element (p, h1-h6, li, blockquote, etc.)
      const fragmentTags = [
        "P",
        "H1",
        "H2",
        "H3",
        "H4",
        "H5",
        "H6",
        "BLOCKQUOTE",
        "LI",
        "PRE",
        "TABLE",
        "FIGURE",
        "DIV",
        "SPAN",
      ];
      let targetElement = e.target;

      // Walk up the DOM to find a suitable fragment element
      while (targetElement && targetElement !== container) {
        if (fragmentTags.includes(targetElement.tagName)) {
          // Skip the reading-content container itself and book-section containers
          if (
            !targetElement.classList.contains("reading-content") &&
            !targetElement.classList.contains("book-section") &&
            !targetElement.classList.contains("section-content")
          ) {
            break;
          }
        }
        targetElement = targetElement.parentElement;
      }

      // If we found a valid target element (not the container itself)
      if (
        targetElement &&
        targetElement !== container &&
        !targetElement.classList.contains("reading-content") &&
        !targetElement.classList.contains("book-section") &&
        !targetElement.classList.contains("section-content")
      ) {
        e.preventDefault();
        e.stopPropagation();

        // Ensure this element has a fragment ID
        if (!targetElement.getAttribute("data-fragment-id")) {
          targetElement.setAttribute("data-fragment-id", generateUUID());
        }

        activeFragmentId = targetElement.getAttribute("data-fragment-id");

        // Highlight the selected fragment
        container
          .querySelectorAll(".fragment-selected")
          .forEach((el) => el.classList.remove("fragment-selected"));
        targetElement.classList.add("fragment-selected");

        showContextMenu(e.clientX, e.clientY);
        return false;
      }

      // If no suitable element found, still show context menu on the clicked element
      // This handles cases where content is just text nodes or unknown elements
      if (e.target !== container) {
        e.preventDefault();
        e.stopPropagation();

        // Use the closest parent with actual content
        let contentElement = e.target;
        while (contentElement.nodeType === 3) {
          // Text node
          contentElement = contentElement.parentElement;
        }

        if (contentElement && contentElement !== container) {
          if (!contentElement.getAttribute("data-fragment-id")) {
            contentElement.setAttribute("data-fragment-id", generateUUID());
          }

          activeFragmentId = contentElement.getAttribute("data-fragment-id");

          container
            .querySelectorAll(".fragment-selected")
            .forEach((el) => el.classList.remove("fragment-selected"));
          contentElement.classList.add("fragment-selected");

          showContextMenu(e.clientX, e.clientY);
          return false;
        }
      }
    });

    // Hide menu on click outside
    document.addEventListener("click", function (e) {
      if (!e.target.closest("#fragment-context-menu")) {
        hideContextMenu();
        // Remove selection highlight
        container
          .querySelectorAll(".fragment-selected")
          .forEach((el) => el.classList.remove("fragment-selected"));
      }
    });

    // Hide on escape
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") {
        hideContextMenu();
        container
          .querySelectorAll(".fragment-selected")
          .forEach((el) => el.classList.remove("fragment-selected"));
      }
    });

    // Hide on scroll
    container.addEventListener("scroll", function () {
      hideContextMenu();
    });
  }

  function ensureFragmentIds(container) {
    const fragmentTags = [
      "p",
      "h1",
      "h2",
      "h3",
      "h4",
      "h5",
      "h6",
      "blockquote",
      "li",
      "pre",
      "table",
      "figure",
    ];
    fragmentTags.forEach((tag) => {
      container.querySelectorAll(tag).forEach((element) => {
        if (!element.getAttribute("data-fragment-id")) {
          element.setAttribute("data-fragment-id", generateUUID());
        }
      });
    });
  }

  function showContextMenu(x, y) {
    const menu = document.getElementById("fragment-context-menu");
    if (menu) {
      menu.style.left = x + "px";
      menu.style.top = y + "px";
      menu.classList.add("visible");
    }
  }

  function hideContextMenu() {
    const menu = document.getElementById("fragment-context-menu");
    if (menu) {
      menu.classList.remove("visible");
    }
  }

  function setupFragmentObserver(container) {
    if (fragmentObserver) {
      fragmentObserver.disconnect();
    }

    const options = {
      root: null,
      rootMargin: "0px",
      threshold: 0.5,
    };

    fragmentObserver = new IntersectionObserver((entries) => {
      const visibleFragments = entries
        .filter((entry) => entry.isIntersecting)
        .map((entry) => entry.target.getAttribute("data-fragment-id"));

      if (visibleFragments.length > 0 && dotNetRefFeedback) {
        dotNetRefFeedback.invokeMethodAsync(
          "OnVisibleFragmentsChanged",
          visibleFragments,
        );
      }
    }, options);

    // Observe all fragments
    container.querySelectorAll("[data-fragment-id]").forEach((fragment) => {
      fragmentObserver.observe(fragment);
    });
  }

  function generateUUID() {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
      /[xy]/g,
      function (c) {
        const r = (Math.random() * 16) | 0;
        const v = c === "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
      },
    );
  }
})();

// CSS for fragment feedback (inject dynamically)
(function () {
  const style = document.createElement("style");
  style.textContent = `
        .fragment-context-menu {
            position: fixed;
            background: var(--neutral-layer-floating, #fff);
            border: 1px solid var(--neutral-stroke-rest, #e0e0e0);
            border-radius: 8px;
            box-shadow: 0 4px 16px rgba(0,0,0,0.15);
            padding: 8px 0;
            z-index: 10000;
            display: none;
            min-width: 160px;
        }
        
        .fragment-context-menu.visible {
            display: block;
        }
        
        .context-menu-item {
            display: flex;
            align-items: center;
            gap: 8px;
            width: 100%;
            padding: 10px 16px;
            border: none;
            background: transparent;
            cursor: pointer;
            font-size: 14px;
            color: var(--neutral-foreground-rest, #333);
            text-align: left;
        }
        
        .context-menu-item:hover {
            background: var(--neutral-layer-1, #f5f5f5);
        }
        
        .context-icon {
            font-size: 16px;
        }
        
        .fragment-highlighted {
            background: rgba(255, 235, 59, 0.3) !important;
            outline: 2px solid #ffc107;
            outline-offset: 2px;
        }
        
        .fragment-selected {
            background: rgba(33, 150, 243, 0.15) !important;
            outline: 2px solid #2196f3;
            outline-offset: 1px;
            border-radius: 4px;
        }
        
        .feedback-indicator {
            position: absolute;
            right: -30px;
            top: 0;
            background: #2196f3;
            color: white;
            border-radius: 50%;
            width: 24px;
            height: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 12px;
            cursor: pointer;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            transition: transform 0.2s;
        }
        
        .feedback-indicator:hover {
            transform: scale(1.1);
        }
        
        .feedback-count {
            font-weight: bold;
        }
        
        [data-fragment-id] {
            cursor: pointer;
            transition: background-color 0.2s;
        }
        
        [data-fragment-id]:hover {
            background-color: rgba(33, 150, 243, 0.05);
        }
    `;
  document.head.appendChild(style);
})();

// ========================================
// Bootstrap Tooltip Global Management
// ========================================
(function () {
  let tooltipInstances = new Map();

  /**
   * Initialize all tooltips on the page
   */
  window.initializeTooltips = function () {
    // Dispose existing tooltips first
    disposeAllTooltips();

    // Find all tooltip triggers
    const tooltipTriggerList = [].slice.call(
      document.querySelectorAll('[data-bs-toggle="tooltip"]'),
    );

    // Initialize new tooltips
    tooltipTriggerList.forEach((tooltipTriggerEl) => {
      try {
        const tooltip = new bootstrap.Tooltip(tooltipTriggerEl, {
          trigger: "hover focus",
          container: "body",
          boundary: "window",
        });
        tooltipInstances.set(tooltipTriggerEl, tooltip);
      } catch (e) {
        console.warn("Failed to initialize tooltip:", e);
      }
    });
  };

  /**
   * Dispose all active tooltips
   */
  function disposeAllTooltips() {
    tooltipInstances.forEach((tooltip, element) => {
      try {
        tooltip.dispose();
      } catch (e) {
        // Tooltip may already be disposed
      }
    });
    tooltipInstances.clear();

    // Remove any orphaned tooltip elements
    document.querySelectorAll(".tooltip").forEach((el) => el.remove());
  }

  window.disposeTooltips = disposeAllTooltips;

  // Auto-cleanup on navigation
  window.addEventListener("beforeunload", disposeAllTooltips);

  // Initialize on DOMContentLoaded
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", window.initializeTooltips);
  }
})();

// Browser history navigation
window.goBack = function () {
  window.history.back();
};

window.goForward = function () {
  window.history.forward();
};

window.canGoBack = function () {
  return window.history.length > 1;
};

// Google OAuth functions
window.googleAuth = {
  client: null,

  initializeGoogleAuth: function (clientId, scope = "openid email profile") {
    return new Promise((resolve, reject) => {
      if (typeof google === "undefined") {
        reject("Google Identity Services not loaded");
        return;
      }

      try {
        this.client = google.accounts.oauth2.initCodeClient({
          client_id: clientId,
          scope: scope,
          ux_mode: "popup",
          callback: (response) => {
            if (response.code) {
              this.handleAuthCallback(response.code);
            } else {
              console.error("Google Auth Error:", response);
            }
          },
          error_callback: (error) => {
            console.error("Google Auth Error:", error);
            this.notifyAuthError(error);
          },
        });
        resolve(true);
      } catch (error) {
        console.error("Failed to initialize Google Auth:", error);
        reject(error);
      }
    });
  },

  signIn: function () {
    if (this.client) {
      try {
        this.client.requestCode();
      } catch (error) {
        console.error("Error requesting Google auth code:", error);
        this.notifyAuthError(error);
      }
    } else {
      console.error("Google Auth client not initialized");
      this.notifyAuthError("Google Auth client not initialized");
    }
  },

  handleAuthCallback: function (code) {
    // Notify Blazor about successful auth code
    if (window.DotNet && window.blazorGoogleAuthCallback) {
      window.blazorGoogleAuthCallback.invokeMethodAsync(
        "OnGoogleAuthSuccess",
        code,
      );
    } else {
      console.log("Google Auth Code received:", code);
      // Fallback - store in sessionStorage for Blazor to retrieve
      sessionStorage.setItem("googleAuthCode", code);
      window.dispatchEvent(
        new CustomEvent("googleAuthSuccess", { detail: code }),
      );
    }
  },

  notifyAuthError: function (error) {
    if (window.DotNet && window.blazorGoogleAuthCallback) {
      window.blazorGoogleAuthCallback.invokeMethodAsync(
        "OnGoogleAuthError",
        error.toString(),
      );
    } else {
      console.error("Google Auth Error:", error);
      window.dispatchEvent(
        new CustomEvent("googleAuthError", { detail: error }),
      );
    }
  },
};

// Check if chips container overflows (~3 lines threshold)
window.assetlenCheckChipsOverflow = (element) => {
    if (!element) return 0;
    return element.scrollHeight;
};


// Initialize Bootstrap tooltips
window.initializeTooltips = function () {
    try {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
        }
    } catch (error) {
        console.error('Error initializing tooltips:', error);
    }
};

// Dispose Bootstrap tooltips
window.disposeTooltips = function () {
    try {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            tooltipTriggerList.forEach(tooltipTriggerEl => {
                const tooltip = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
                if (tooltip) {
                    tooltip.dispose();
                }
            });
        }
    } catch (error) {
        console.error('Error disposing tooltips:', error);
    }
};

// Initialize navbar collapse handler
window.initNavbarCollapse = function () {
    try {
        // Add click event listener to all nav links
        const navLinks = document.querySelectorAll('#navbarNavDropdown .nav-link-item');
        const navbarCollapse = document.getElementById('navbarNavDropdown');

        if (navbarCollapse && typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
            const bsCollapse = new bootstrap.Collapse(navbarCollapse, { toggle: false });

            navLinks.forEach(link => {
                link.addEventListener('click', function () {
                    // Only collapse on mobile/tablet (when navbar is in collapsed mode)
                    if (window.innerWidth < 992) { // Bootstrap's lg breakpoint
                        bsCollapse.hide();
                    }
                });
            });
        }
    } catch (error) {
        console.error('Error initializing navbar collapse:', error);
    }
};

// Programmatically collapse the navbar (called from Blazor)
window.collapseNavbar = function () {
    try {
        const navbarCollapse = document.getElementById('navbarNavDropdown');

        if (navbarCollapse && typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
            // Only collapse on mobile/tablet (when navbar is in collapsed mode)
            if (window.innerWidth < 992) { // Bootstrap's lg breakpoint
                const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse) || new bootstrap.Collapse(navbarCollapse, { toggle: false });
                bsCollapse.hide();
            }
        }
    } catch (error) {
        console.error('Error collapsing navbar:', error);
    }
};

/* ─────────────────────────────────────────────────────────────────────────
   ASSETLEN theme

   Three states, matching the CSS in app.css:
     null    → follow the operating system (no attribute on <html>)
     "light" → data-theme="light"
     "dark"  → data-theme="dark"

   The <meta name="theme-color"> pair is updated alongside so the phone's
   browser chrome matches the page instead of sitting a shade off it.
   ───────────────────────────────────────────────────────────────────────── */
window.assetlenTheme = {
  apply: function (theme) {
    var root = document.documentElement;

    if (theme === "light" || theme === "dark") {
      root.setAttribute("data-theme", theme);
    } else {
      root.removeAttribute("data-theme");
    }

    var dark =
      theme === "dark" ||
      (!theme && window.matchMedia("(prefers-color-scheme: dark)").matches);

    var meta = document.querySelector('meta[name="theme-color"]:not([media])');
    if (!meta) {
      meta = document.createElement("meta");
      meta.setAttribute("name", "theme-color");
      document.head.appendChild(meta);
    }
    meta.setAttribute("content", dark ? "#131518" : "#f6f5f1");
  },
};

/* Apply the stored choice before Blazor boots, so the splash and the first
   paint are already in the right theme rather than flashing white. */
(function () {
  try {
    var stored = window.localStorage.getItem("al-theme");
    if (stored) {
      // Blazored.LocalStorage stores strings JSON-quoted.
      var value = stored.replace(/^"|"$/g, "");
      if (value === "light" || value === "dark") {
        document.documentElement.setAttribute("data-theme", value);
      }
    }
  } catch (e) {
    /* private mode, or storage disabled — the OS preference still governs */
  }
})();

/* ─────────────────────────────────────────────────────────────────────────
   ASSETLEN — window.assetlen

   Everything the Razor components reach for through IJSRuntime lives on this
   one object. Two members so far: the lightbox key handler, and the tab-strip
   edge helper.
   ───────────────────────────────────────────────────────────────────────── */
window.assetlen = window.assetlen || {};

/* ── Lightbox ──────────────────────────────────────────────────────────────
   PhotoLightbox calls attach/detach and expects arrow keys and Escape to come
   back through [JSInvokable] OnKey. These were being called and did not exist,
   so opening a frame threw out of OnAfterRenderAsync.
   ────────────────────────────────────────────────────────────────────────── */
window.assetlen.lightbox = (function () {
  var ref = null;
  var handler = null;

  return {
    attach: function (dotNetRef) {
      if (handler) document.removeEventListener("keydown", handler);

      ref = dotNetRef;
      handler = function (e) {
        if (e.key !== "Escape" && e.key !== "ArrowLeft" && e.key !== "ArrowRight") return;

        // Arrow keys scroll the page behind the lightbox otherwise.
        e.preventDefault();
        if (ref) ref.invokeMethodAsync("OnKey", e.key);
      };

      document.addEventListener("keydown", handler);
    },

    detach: function () {
      if (handler) document.removeEventListener("keydown", handler);
      handler = null;
      ref = null;
    },
  };
})();

/* ── Tab strip edges ───────────────────────────────────────────────────────
   A horizontally scrolling strip on a phone has one failure mode that matters:
   the reader tries to swipe it and instead taps a tab, because a short drag on
   a link reads as a tap. So the strip gets explicit chevrons — and a chevron
   that is always visible is a chevron nobody trusts, so this reports which
   direction actually has more strip left.

   Scroll is driven off tab geometry rather than a fixed number of pixels: the
   next press brings the first partly-hidden tab fully into view. Paging by a
   percentage lands mid-label, which is what makes most of these feel cheap.
   ────────────────────────────────────────────────────────────────────────── */
window.assetlen.tabstrip = (function () {
  var strips = new Map();
  var SLACK = 2; // sub-pixel scroll positions never land exactly on 0

  function measure(el) {
    var max = el.scrollWidth - el.clientWidth;
    return {
      overflows: max > SLACK,
      atStart: el.scrollLeft <= SLACK,
      atEnd: el.scrollLeft >= max - SLACK,
    };
  }

  function report(id) {
    var entry = strips.get(id);
    if (!entry) return;

    var m = measure(entry.el);
    if (entry.last &&
        entry.last.overflows === m.overflows &&
        entry.last.atStart === m.atStart &&
        entry.last.atEnd === m.atEnd) {
      return; // nothing changed — do not wake the renderer
    }

    entry.last = m;
    entry.ref.invokeMethodAsync("OnEdgesChanged", m.overflows, m.atStart, m.atEnd);
  }

  /* Geometry from rects, not offsetLeft.
     offsetLeft is measured against offsetParent, and the strip's wrapper is
     position:relative — so the tabs answer relative to the wrapper rather than
     to the scroller, every tab reports the same left as the strip's own, and
     "the first tab past the right edge" is never found. Rects plus scrollLeft
     are unambiguous whatever is positioned above. */
  function tabs(el) {
    var railLeft = el.getBoundingClientRect().left;
    var scrolled = el.scrollLeft;

    return Array.prototype.slice.call(el.children)
      .filter(function (c) { return c.getBoundingClientRect().width > 0; })
      .map(function (c) {
        var r = c.getBoundingClientRect();
        var start = r.left - railLeft + scrolled;   // position in the scroll content
        return { el: c, start: start, end: start + r.width, width: r.width };
      });
  }

  return {
    register: function (id, el, dotNetRef) {
      if (!el) return;

      var entry = { el: el, ref: dotNetRef, last: null };
      strips.set(id, entry);

      entry.onScroll = function () { report(id); };
      el.addEventListener("scroll", entry.onScroll, { passive: true });

      if (window.ResizeObserver) {
        entry.observer = new ResizeObserver(function () { report(id); });
        entry.observer.observe(el);
        // Adding or removing a tab changes the overflow without resizing the rail.
        Array.prototype.forEach.call(el.children, function (c) { entry.observer.observe(c); });
      }

      report(id);
    },

    unregister: function (id) {
      var entry = strips.get(id);
      if (!entry) return;

      entry.el.removeEventListener("scroll", entry.onScroll);
      if (entry.observer) entry.observer.disconnect();
      strips.delete(id);
    },

    /* direction: -1 back, 1 forward. Lands on a tab edge, never mid-label. */
    step: function (id, direction) {
      var entry = strips.get(id);
      if (!entry) return;

      var el = entry.el;
      var children = tabs(el);
      if (!children.length) return;

      var viewStart = el.scrollLeft;
      var viewEnd = viewStart + el.clientWidth;
      var max = el.scrollWidth - el.clientWidth;

      // The strip carries scroll-padding so a tab never lands under a chevron,
      // and the tabs are snap-aligned to `start`. Every target below is
      // therefore a tab's leading edge less that padding — i.e. an actual snap
      // position. Aiming anywhere else is silently undone: an earlier version
      // put the incoming tab flush against the *right* edge, which fell between
      // two snap points, and proximity snapping pulled the strip straight back
      // to where it started. The chevron looked dead.
      var pad = parseFloat(getComputedStyle(el).scrollPaddingLeft) || 0;
      var target = null;

      if (direction > 0) {
        // The first tab that is not fully in view becomes the leading tab.
        // Nothing is skipped: it was already partly visible.
        for (var i = 0; i < children.length; i++) {
          if (children[i].end > viewEnd + SLACK) {
            target = children[i].start - pad;
            break;
          }
        }
        if (target === null) target = max;
      } else {
        for (var j = children.length - 1; j >= 0; j--) {
          if (children[j].start - pad < viewStart - SLACK) {
            target = children[j].start - pad;
            break;
          }
        }
        if (target === null) target = 0;
      }

      target = Math.max(0, Math.min(target, max));

      // One very wide tab can leave the computed target where we already are.
      // Finish the journey rather than let a live chevron do nothing.
      if (Math.abs(target - viewStart) <= SLACK) target = direction > 0 ? max : 0;

      el.scrollTo({ left: target, behavior: "smooth" });
    },

    /* Landing on a section whose tab is scrolled out of sight reads as "this
       section is not in the strip". Called once after the strip mounts. */
    revealActive: function (id) {
      var entry = strips.get(id);
      if (!entry) return;

      var el = entry.el;
      var active = tabs(el).filter(function (t) {
        return t.el.classList.contains("is-active");
      })[0];
      if (!active) return;

      var viewStart = el.scrollLeft;
      var viewEnd = viewStart + el.clientWidth;

      if (active.start >= viewStart - SLACK && active.end <= viewEnd + SLACK) return;

      // Centre it when there is room either side; otherwise just bring it in.
      var centred = active.start - (el.clientWidth - active.width) / 2;
      el.scrollTo({
        left: Math.max(0, Math.min(centred, el.scrollWidth - el.clientWidth)),
        behavior: "auto",
      });
    },
  };
})();
