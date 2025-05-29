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

    def generate_pdf_report(self):
        products = self.db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
        products_sorted = sorted(products, key=lambda x: x[1], reverse=True)
        total = len(products_sorted)
        a_cut = int(total * 0.2)
        b_cut = int(total * 0.5)

        pdf = FPDF()
        pdf.add_page()
        pdf.set_font("Arial", "B", 16)
        pdf.cell(0, 10, "Reporte de Inventario", ln=True, align="C")
        pdf.ln(10)

        # Sección de alertas
        pdf.set_font("Arial", "B", 12)
        pdf.cell(0, 10, "Alertas:", ln=True)
        hay_alertas = False
        for product in products_sorted:
            if product[1] < product[2]:
                alerta = f"¡Alerta! El producto '{product[0]}' está por debajo del umbral de reposición."
                pdf.set_text_color(255, 0, 0)
                pdf.cell(0, 10, alerta, ln=True)
                hay_alertas = True
        if not hay_alertas:
            pdf.set_text_color(0, 128, 0)
            pdf.cell(0, 10, "No hay alertas.", ln=True)
        pdf.set_text_color(0, 0, 0)
        pdf.ln(5)

        # Sección de inventario con clasificación ABC
        pdf.set_font("Arial", "B", 12)
        pdf.cell(0, 10, "Inventario (Clasificación ABC):", ln=True)
        pdf.set_font("Arial", "B", 10)
        pdf.cell(50, 10, "Nombre", 1)
        pdf.cell(30, 10, "Cantidad", 1)
        pdf.cell(30, 10, "Umbral", 1)
        pdf.cell(20, 10, "Clase", 1)
        pdf.ln()
        pdf.set_font("Arial", "", 10)
        for idx, product in enumerate(products_sorted):
            if idx < a_cut:
                clase = "A"
            elif idx < b_cut:
                clase = "B"
            else:
                clase = "C"
            pdf.cell(50, 10, str(product[0]), 1)
            pdf.cell(30, 10, str(product[1]), 1)
            pdf.cell(30, 10, str(product[2]), 1)
            pdf.cell(20, 10, clase, 1)
            pdf.ln()

        pdf.output("data/report.pdf")
        print(f"Reporte PDF generado en: {'data/report.pdf'}")