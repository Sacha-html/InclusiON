import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

export interface PdfExportOptions {
  filename?: string;
  orientation?: 'portrait' | 'landscape';
  format?: 'a4' | 'letter';
  margin?: number; // margen en mm (por defecto 10mm)
  fitToSinglePage?: boolean; // si es true, ajusta el tamaño proporcionalmente para que entre en 1 página A4
  scale?: number; // resolución de renderizado (por defecto 2 para alta definición)
}

/**
 * Convierte cualquier color CSS (incluso funciones modernas como color(srgb...), color-mix(...), etc.)
 * a formato rgba(...) seguro utilizando un canvas 2D auxiliar.
 */
function resolveSafeColor(colorStr: string, fallback: string = '#ffffff'): string {
  if (!colorStr || colorStr === 'transparent' || colorStr === 'inherit') {
    return colorStr;
  }

  // Si no contiene funciones problemáticas modernas, lo devolvemos tal cual
  if (!colorStr.includes('color(') && !colorStr.includes('color-mix(') && !colorStr.includes('oklch') && !colorStr.includes('lab(')) {
    return colorStr;
  }

  try {
    const canvas = document.createElement('canvas');
    canvas.width = 1;
    canvas.height = 1;
    const ctx = canvas.getContext('2d');
    if (!ctx) return fallback;

    ctx.fillStyle = colorStr;
    ctx.fillRect(0, 0, 1, 1);
    const [r, g, b, a] = ctx.getImageData(0, 0, 1, 1).data;
    const alpha = (a / 255).toFixed(2);
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
  } catch {
    return fallback;
  }
}

/**
 * Captura un elemento HTML del DOM y lo descarga como archivo PDF en formato A4 exacto,
 * sanitizando estilos CSS modernos incompatibles con html2canvas y ajustando la paginación a la hoja A4.
 */
export async function exportHtmlElementToPdf(
  element: HTMLElement,
  options?: PdfExportOptions
): Promise<void> {
  const filename = options?.filename || `Reporte_${new Date().toISOString().slice(0, 10)}.pdf`;
  const orientation = options?.orientation || 'portrait';
  const format = options?.format || 'a4';
  const margin = options?.margin !== undefined ? options.margin : 10;
  const scale = options?.scale || 2;
  const fitToSinglePage = options?.fitToSinglePage ?? false;

  // 1. Renderizar el elemento a canvas con html2canvas sanitizando colores
  const canvas = await html2canvas(element, {
    scale,
    useCORS: true,
    logging: false,
    backgroundColor: '#ffffff',
    windowWidth: 1280, // Asegura que los componentes Bootstrap/CoreUI mantengan su grilla de escritorio
    onclone: (clonedDoc: Document, clonedElement: HTMLElement) => {
      // Inyectar estilos de anulación para variables CSS problemáticas
      const overrideStyle = clonedDoc.createElement('style');
      overrideStyle.textContent = `
        :root, html, body {
          --cui-primary-bg-subtle: rgba(13, 110, 253, 0.12) !important;
          --cui-success-bg-subtle: rgba(25, 135, 84, 0.12) !important;
          --cui-danger-bg-subtle: rgba(220, 53, 69, 0.12) !important;
          --cui-warning-bg-subtle: rgba(255, 193, 7, 0.12) !important;
          --cui-info-bg-subtle: rgba(13, 202, 240, 0.12) !important;
          --cui-body-bg: #ffffff !important;
          --cui-body-color: #212529 !important;
          --cui-border-color: #dee2e6 !important;
        }
        * {
          text-shadow: none !important;
        }
      `;
      clonedDoc.head.appendChild(overrideStyle);

      // Sanitizar propiedades de color en todos los elementos clonados
      const elementsToSanitize = [clonedElement, ...Array.from(clonedElement.querySelectorAll<HTMLElement>('*'))];

      for (const el of elementsToSanitize) {
        if (!el || !el.style) continue;

        try {
          const computed = window.getComputedStyle(el);

          // Background
          const bg = computed.backgroundColor;
          if (bg && (bg.includes('color(') || bg.includes('color-mix(') || bg.includes('oklch') || bg.includes('lab('))) {
            el.style.backgroundColor = resolveSafeColor(bg, '#ffffff');
          }

          // Text Color
          const fg = computed.color;
          if (fg && (fg.includes('color(') || fg.includes('color-mix(') || fg.includes('oklch') || fg.includes('lab('))) {
            el.style.color = resolveSafeColor(fg, '#212529');
          }

          // Border Color
          const bc = computed.borderColor;
          if (bc && (bc.includes('color(') || bc.includes('color-mix(') || bc.includes('oklch') || bc.includes('lab('))) {
            el.style.borderColor = resolveSafeColor(bc, '#dee2e6');
          }

          // Box shadow con colores modernos
          const shadow = computed.boxShadow;
          if (shadow && (shadow.includes('color(') || shadow.includes('color-mix(') || shadow.includes('oklch') || shadow.includes('lab('))) {
            el.style.boxShadow = 'none';
          }
        } catch {
          // Ignorar elementos no accesibles
        }
      }
    },
  });

  // 2. Inicializar documento jsPDF en A4 (210 x 297 mm en Portrait o 297 x 210 mm en Landscape)
  const pdf = new jsPDF({
    orientation,
    unit: 'mm',
    format: 'a4',
    compress: true,
  });

  const pageWidth = pdf.internal.pageSize.getWidth();   // 210mm (Portrait) o 297mm (Landscape)
  const pageHeight = pdf.internal.pageSize.getHeight(); // 297mm (Portrait) o 210mm (Landscape)
  const printWidth = pageWidth - margin * 2;
  const printHeight = pageHeight - margin * 2;

  // 3. Ajuste de contenido en A4
  if (fitToSinglePage) {
    // Escalar para que encaje exactamente en 1 sola hoja A4
    const scaleFactor = Math.min(printWidth / canvas.width, printHeight / canvas.height);
    const renderWidth = canvas.width * scaleFactor;
    const renderHeight = canvas.height * scaleFactor;
    const posX = margin + (printWidth - renderWidth) / 2;
    const posY = margin + (printHeight - renderHeight) / 2;

    const imgData = canvas.toDataURL('image/png');
    pdf.addImage(imgData, 'PNG', posX, posY, renderWidth, renderHeight, undefined, 'FAST');
  } else {
    // Renderizado proporcional manteniendo el ancho de impresión A4
    const imgWidthMm = printWidth;
    const imgHeightMm = (canvas.height * printWidth) / canvas.width;

    if (imgHeightMm <= printHeight) {
      // Cabe perfectamente en 1 sola página A4
      const imgData = canvas.toDataURL('image/png');
      pdf.addImage(imgData, 'PNG', margin, margin, imgWidthMm, imgHeightMm, undefined, 'FAST');
    } else {
      // Paginación multipágina en A4 cortando por bloques exactos
      const pageCanvasHeight = (printHeight * canvas.width) / printWidth;
      let renderedHeight = 0;
      let pageIndex = 0;

      while (renderedHeight < canvas.height) {
        if (pageIndex > 0) {
          pdf.addPage('a4', orientation);
        }

        const currentSliceHeight = Math.min(pageCanvasHeight, canvas.height - renderedHeight);

        // Crear canvas temporal para la porción de la página A4
        const pageCanvas = document.createElement('canvas');
        pageCanvas.width = canvas.width;
        pageCanvas.height = currentSliceHeight;
        const pageCtx = pageCanvas.getContext('2d');

        if (pageCtx) {
          pageCtx.fillStyle = '#ffffff';
          pageCtx.fillRect(0, 0, pageCanvas.width, pageCanvas.height);
          pageCtx.drawImage(
            canvas,
            0, renderedHeight, canvas.width, currentSliceHeight,
            0, 0, canvas.width, currentSliceHeight
          );

          const sliceData = pageCanvas.toDataURL('image/png');
          const sliceHeightMm = (currentSliceHeight * printWidth) / canvas.width;
          pdf.addImage(sliceData, 'PNG', margin, margin, printWidth, sliceHeightMm, undefined, 'FAST');
        }

        renderedHeight += currentSliceHeight;
        pageIndex++;
      }
    }
  }

  // 4. Descargar el archivo PDF A4 generado
  pdf.save(filename);
}
