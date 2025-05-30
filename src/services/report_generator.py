import json
from fpdf import FPDF

class ReportGenerator:
    def __init__(self, db_manager):
        self.db_manager = db_manager

    def generate_report(self):
        products = self.db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
        # Clasificación ABC
        products_sorted = sorted(products, key=lambda x: x[1], reverse=True)
        total = len(products_sorted)
        a_cut = int(total * 0.2)
        b_cut = int(total * 0.5)
        inventario = []
        alertas = []
        for idx, product in enumerate(products_sorted):
            if idx < a_cut:
                clase = "A"
            elif idx < b_cut:
                clase = "B"
            else:
                clase = "C"
            prod_dict = {
                "nombre": product[0],
                "cantidad": product[1],
                "umbral": product[2],
                "clase": clase
            }
            inventario.append(prod_dict)
            if product[1] < product[2]:
                alerta = f"¡Alerta! El producto '{product[0]}' está por debajo del umbral de reposición."
                alertas.append(alerta)
        reporte = {
            "inventario": inventario,
            "alertas": alertas
        }
        with open("data/report.json", "w", encoding="utf-8") as f:
            json.dump(reporte, f, indent=4, ensure_ascii=False)

    def generate_json_report(self, file_path):
        inventory_data = self.db_manager.fetch_inventory_data()
        with open(file_path, 'w') as json_file:
            json.dump(inventory_data, json_file, indent=4)

    def generate_pdf_report(self, texto_ia=None):
        products = self.db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
        pdf = FPDF()
        pdf.add_page()
        pdf.set_font("Arial", "B", 16)
        pdf.cell(0, 10, "Reporte de Inventario", ln=True, align="C")
        pdf.ln(10)

        # Tabla de inventario
        pdf.set_font("Arial", "B", 12)
        pdf.cell(60, 10, "Producto", 1)
        pdf.cell(30, 10, "Cantidad", 1)
        pdf.cell(30, 10, "Umbral", 1)
        pdf.ln()
        pdf.set_font("Arial", "", 12)
        for p in products:
            pdf.cell(60, 10, str(p[0]), 1)
            pdf.cell(30, 10, str(p[1]), 1)
            pdf.cell(30, 10, str(p[2]), 1)
            pdf.ln()

        # Si hay texto de IA, lo agregamos al final
        if texto_ia:
            pdf.ln(10)
            pdf.set_font("Arial", "B", 12)
            pdf.cell(0, 10, "Análisis de IA:", ln=True)
            pdf.set_font("Arial", "", 12)
            pdf.multi_cell(0, 10, texto_ia)

        pdf.output("data/report.pdf")
        print(f"Reporte PDF generado en: {'data/report.pdf'}")