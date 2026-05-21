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
