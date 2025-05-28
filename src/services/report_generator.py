import json
from fpdf import FPDF

class ReportGenerator:
    def __init__(self, db_manager):
        self.db_manager = db_manager

    def generate_report(self, file_path='data/report.json'):
        """Genera un reporte del inventario en formato JSON."""
        query = "SELECT id, product_name, quantity, restock_threshold FROM inventory"
        inventory_data = self.db_manager.fetch_query(query)

        report = [
            {
                "id": item[0],
                "product_name": item[1],
                "quantity": item[2],
                "restock_threshold": item[3],
            }
            for item in inventory_data
        ]

        with open(file_path, 'w') as json_file:
            json.dump(report, json_file, indent=4)
        print(f"Reporte generado en: {file_path}")

    def generate_json_report(self, file_path):
        inventory_data = self.db_manager.fetch_inventory_data()
        with open(file_path, 'w') as json_file:
            json.dump(inventory_data, json_file, indent=4)

    def generate_pdf_report(self, file_path='data/report.pdf'):
        """Genera un reporte del inventario en formato PDF."""
        query = "SELECT product_name, quantity, restock_threshold FROM inventory"
        inventory_data = self.db_manager.fetch_query(query)

        pdf = FPDF()
        pdf.add_page()
        pdf.set_font("Arial", size=12)

        # Título del reporte
        pdf.set_font("Arial", style="B", size=16)
        pdf.cell(200, 10, txt="Reporte de Inventario", ln=True, align="C")
        pdf.ln(10)  # Espaciado

        # Encabezados de la tabla
        pdf.set_font("Arial", style="B", size=12)
        pdf.cell(80, 10, txt="Producto", border=1)
        pdf.cell(40, 10, txt="Cantidad", border=1)
        pdf.cell(70, 10, txt="Umbral de Reposición", border=1)
        pdf.ln()

        # Datos del inventario
        pdf.set_font("Arial", size=12)
        for product_name, quantity, restock_threshold in inventory_data:
            pdf.cell(80, 10, txt=product_name, border=1)
            pdf.cell(40, 10, txt=str(quantity), border=1)
            pdf.cell(70, 10, txt=str(restock_threshold), border=1)
            pdf.ln()

        # Guardar el archivo PDF
        pdf.output(file_path)
        print(f"Reporte PDF generado en: {file_path}")