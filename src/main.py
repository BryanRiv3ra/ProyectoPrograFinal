# main.py

from agents.inventory_agent import InventoryAgent
from agents.prediction_agent import PredictionAgent
from database.db_manager import DBManager
from services.event_logger import EventLogger
from services.report_generator import ReportGenerator
from colorama import Fore, Style
from tabulate import tabulate
import os
import sqlite3

def main():
    db_path = os.path.abspath('data/inventory.db')
    print(f"Ruta completa de la base de datos: {db_path}")

    # Inicializar la base de datos
    db_manager = DBManager(db_path)
    db_manager.initialize_database()

    # Crear instancias de los agentes y servicios
    inventory_agent = InventoryAgent(db_manager)
    prediction_agent = PredictionAgent(db_manager)
    event_logger = EventLogger()
    report_generator = ReportGenerator(db_manager)

    while True:
        print("\n--- Sistema de Inventario ---")
        print("1. Agregar producto")
        print("2. Actualizar producto")
        print("3. Verificar inventario")
        print("4. Generar reporte")
        print("5. Predicción de reabastecimiento")
        print("6. Salir")

        choice = input("Selecciona una opción: ")

        if choice == "1":
            product_name = input("Nombre del producto: ")
            quantity = get_positive_int("Cantidad: ")
            restock_threshold = get_positive_int("Umbral de reposición: ")
            try:
                inventory_agent.add_product(product_name, quantity, restock_threshold)
                print(Fore.GREEN + "Producto agregado con éxito." + Style.RESET_ALL)
            except Exception as e:
                print(Fore.RED + f"Error: No se pudo agregar el producto. {e}" + Style.RESET_ALL)
        elif choice == "2":
            product_name = input("Nombre del producto: ")
            new_quantity = get_positive_int("Nueva cantidad: ")
            inventory_agent.update_product(product_name, new_quantity)
            print(f"Producto '{product_name}' actualizado con éxito.")
        elif choice == "3":
            products = db_manager.fetch_query("SELECT * FROM inventory")
            headers = ["ID", "Nombre", "Cantidad", "Umbral"]
            print("\n--- Inventario Actual ---")
            print(tabulate(products, headers=headers, tablefmt="grid"))
        elif choice == "4":
            report_generator.generate_report()
            report_generator.generate_pdf_report()
            print("Reportes generados con éxito (JSON y PDF).")
        elif choice == "5":
            predictions = prediction_agent.predict_restock()
            print("\n--- Predicciones de Reabastecimiento ---")
            for prediction in predictions:
                print(f"Producto: {prediction['product_name']}, Reabastecer: {prediction['restock_amount']} unidades.")
                event_logger.log_event(f"Predicción: {prediction['product_name']} necesita reabastecer {prediction['restock_amount']} unidades.")
        elif choice == "6":
            print("Saliendo del sistema...")
            break
        else:
            print("Opción no válida. Intenta de nuevo.")

def get_positive_int(prompt):
    while True:
        try:
            value = int(input(prompt))
            if value < 0:
                raise ValueError("El valor debe ser positivo.")
            return value
        except ValueError as e:
            print(f"Entrada inválida: {e}")

def _connect(self):
    """Establece la conexión con la base de datos."""
    try:
        connection = sqlite3.connect(self.db_path)
        # Verificar que el archivo sea una base de datos válida
        connection.execute("PRAGMA integrity_check;")
        return connection
    except sqlite3.DatabaseError as e:
        print(f"Error: El archivo no es una base de datos válida. {e}")
        raise

def add_product(self, product_name, quantity, restock_threshold=10):
    if not product_name or not isinstance(product_name, str):
        raise ValueError("El nombre del producto debe ser una cadena no vacía.")
    if quantity < 0 or restock_threshold < 0:
        raise ValueError("La cantidad y el umbral de reposición deben ser mayores o iguales a 0.")
    # Código existente...

def delete_product(self, product_name):
    query = "DELETE FROM inventory WHERE product_name = ?"
    self.db_manager.execute_query(query, (product_name,))
    print(f"Producto '{product_name}' eliminado del inventario.")

def search_product(self, product_name):
    query = "SELECT * FROM inventory WHERE product_name LIKE ?"
    results = self.db_manager.fetch_query(query, (f"%{product_name}%",))
    return results

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(f"Error inesperado: {e}")
        with open('data/event_log.txt', 'a') as log_file:
            log_file.write(f"[ERROR] {e}\n")