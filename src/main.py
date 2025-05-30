# main.py

from agents.inventory_agent import InventoryAgent
from agents.prediction_agent import PredictionAgent
from agents.openai_agent import OpenAIAgent
from agents.compras_agent import ComprasAgent
from agents.finanzas_agent import FinanzasAgent
from database.db_manager import DBManager
from services.event_logger import EventLogger
from services.report_generator import ReportGenerator
from colorama import Fore, Style
from tabulate import tabulate
from fpdf import FPDF
import os
import sqlite3
import json

def main():
    db_path = os.path.abspath('data/inventory.db')
    print(f"Ruta completa de la base de datos: {db_path}")

    # Inicializar la base de datos
    db_manager = DBManager(db_path)
    db_manager.initialize_database()

    openai_api_key = "apikey"  # ¡Nunca subas tu API key a GitHub!
    openai_agent = OpenAIAgent(openai_api_key)

    # Crear instancias de los agentes y servicios
    inventory_agent = InventoryAgent(db_manager, openai_agent)
    compras_agent = ComprasAgent(openai_agent)
    finanzas_agent = FinanzasAgent(openai_agent)
    prediction_agent = PredictionAgent(openai_agent)
    event_logger = EventLogger()
    report_generator = ReportGenerator(db_manager)

    while True:
        print("\n--- Sistema de Inventario ---")
        print("1. Agregar producto")
        print("2. Actualizar producto")
        print("3. Verificar inventario")
        print("4. Generar reporte")
        print("5. Predicción de reabastecimiento")
        print("6. Eliminar producto")
        print("7. Clasificación ABC (IA)")
        print("8. Reporte ejecutivo (IA)")
        print("9. Sugerencia optima de pedido (IA)")
        print("10. Simulacion de negociacion (IA)")
        print("11. Simulacion Multi agente (IA)")
        print("12. Salir")
        choice = input("Selecciona una opción: ")

        if choice == "1":
            product_name = input("Nombre del producto: ")
            quantity = get_positive_int("Cantidad: ")
            restock_threshold = get_positive_int("Umbral de reposición: ")
            try:
                inventory_agent.add_product(product_name, quantity, restock_threshold)
                print(Fore.GREEN + "Producto agregado con éxito." + Style.RESET_ALL)
                report_generator.generate_report()
                report_generator.generate_pdf_report()
                print(Fore.GREEN + "Reportes actualizados (JSON y PDF)." + Style.RESET_ALL)
                # ALERTA INTELIGENTE: Ahora usando ChatGPT
                products = db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
                inventory_list = [
                    {"nombre": p[0], "cantidad": p[1], "umbral": p[2]}
                    for p in products
                ]
                prompt = (
                    "Analiza el siguiente inventario:\n"
                    f"{inventory_list}\n"
                    "¿Qué productos requieren atención inmediata y por qué? Responde solo con alertas claras."
                )
                respuesta = openai_agent.ask_gpt(prompt)
                print(Fore.YELLOW + "\n--- Alertas de IA (ChatGPT) ---\n" + respuesta + Style.RESET_ALL)
                agregar_respuesta_ia_a_pdf(respuesta, "data/alertas_ia.pdf")
                guardar_respuesta_ia_en_json(respuesta, "data/alertas_ia.json")
            except Exception as e:
                print(Fore.RED + f"Error: No se pudo agregar el producto. {e}" + Style.RESET_ALL)

        elif choice == "2":
            product_id = input("ID del producto a actualizar: ")
            new_quantity = get_positive_int("Nueva cantidad: ")
            inventory_agent.update_product_by_id(product_id, new_quantity)
            print(f"Producto con ID {product_id} actualizado con éxito.")
            # ALERTA INTELIGENTE: Ahora usando ChatGPT
            products = db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
            inventory_list = [
                {"nombre": p[0], "cantidad": p[1], "umbral": p[2]}
                for p in products
            ]
            prompt = (
                "Analiza el siguiente inventario:\n"
                f"{inventory_list}\n"
                "¿Qué productos requieren atención inmediata y por qué? Responde solo con alertas claras."
            )
            respuesta = openai_agent.ask_gpt(prompt)
            print(Fore.YELLOW + "\n--- Alertas de IA (ChatGPT) ---\n" + respuesta + Style.RESET_ALL)
            agregar_respuesta_ia_a_pdf(respuesta, "data/alertas_ia.pdf")
            guardar_respuesta_ia_en_json(respuesta, "data/alertas_ia.json")

        elif choice == "3":
            products = db_manager.fetch_query("SELECT id, product_name, quantity, restock_threshold FROM inventory")
            headers = ["ID", "Nombre", "Cantidad", "Umbral"]
            print("\n--- Inventario Actual ---")
            print(tabulate(products, headers=headers, tablefmt="grid"))
        elif choice == "4":
            report_generator.generate_report()
            report_generator.generate_pdf_report()
            print("Reportes generados con éxito (JSON y PDF).")
        elif choice == "5":
            products = db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
            inventory_list = [
                {"nombre": p[0], "cantidad": p[1], "umbral": p[2]}
                for p in products
            ]
            prompt = (
                "Eres un experto en gestión de inventarios. "
                "Con base en el siguiente inventario, indica qué productos necesitan reabastecimiento, "
                "cuántas unidades sugerirías pedir de cada uno y explica brevemente tu razonamiento. "
                "Responde en formato de lista:\n"
                f"{inventory_list}"
            )
            respuesta = openai_agent.ask_gpt(prompt)
            print(Fore.YELLOW + "\n--- Predicción de Reabastecimiento por IA (ChatGPT) ---\n" + respuesta + Style.RESET_ALL)
            # Si quieres, también puedes guardar la respuesta en PDF o JSON:
            agregar_respuesta_ia_a_pdf(respuesta, "data/prediccion_reabastecimiento_ia.pdf")
            guardar_respuesta_ia_en_json(respuesta, "data/prediccion_reabastecimiento_ia.json")
       
        elif choice == "6":
            product_id = input("ID del producto a eliminar: ")
            try:
                inventory_agent.delete_product_by_id(product_id)
                print(Fore.GREEN + f"Producto con ID {product_id} eliminado con éxito." + Style.RESET_ALL)
                report_generator.generate_report()
                report_generator.generate_pdf_report()
                print(Fore.GREEN + "Reportes actualizados (JSON y PDF)." + Style.RESET_ALL)
            except Exception as e:
                print(Fore.RED + f"Error: No se pudo eliminar el producto. {e}" + Style.RESET_ALL)
        elif choice == "7":
            products = db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
            inventory_list = [
                {"nombre": p[0], "cantidad": p[1], "umbral": p[2]}
                for p in products
            ]
            prompt = (
                "Clasifica los siguientes productos del inventario en categorías ABC según su importancia y cantidad. "
                "Explica brevemente el criterio y muestra la lista con la categoría asignada a cada producto:\n"
                f"{inventory_list}"
            )
            respuesta = openai_agent.ask_gpt(prompt)
            print(Fore.CYAN + "\n--- Clasificación ABC por IA (ChatGPT) ---\n" + respuesta + Style.RESET_ALL)
        elif choice == "8":
            products = db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
            inventory_list = [
                {"nombre": p[0], "cantidad": p[1], "umbral": p[2]}
                for p in products
            ]
            prompt = (
                "Genera un reporte ejecutivo del siguiente inventario. Incluye: "
                "1) Resumen general, 2) Productos críticos, 3) Recomendaciones para optimizar el inventario:\n"
                f"{inventory_list}"
            )
            respuesta = openai_agent.ask_gpt(prompt)
            print(Fore.MAGENTA + "\n--- Reporte Ejecutivo por IA (ChatGPT) ---\n" + respuesta + Style.RESET_ALL)
            report_generator.generate_pdf_report(texto_ia=respuesta)
            print(Fore.GREEN + "Reporte PDF actualizado con el análisis de IA." + Style.RESET_ALL)
            
        elif choice == "9":
            products = db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
            inventory_list = [
                {"nombre": p[0], "cantidad": p[1], "umbral": p[2]}
                for p in products
            ]
            criterios = input("Considera los siguientes criterios para sugerir la cantidad optima de pedido para cada producto:\n"
                             "1) Minimizar el costo total del inventario,\n"
                             "2) Evitar el sobrestock,\n"
                             "3) Mantener un nivel suficiente de stock para evitar quiebres,\n"
                             "4) Optimizar el espacio que hay en el almacén."
                             )
            prompt = (
                "Eres un experto en gestión de inventarios. "
                f"Inventario actual:\n{inventory_list}\n"
                f"{criterios}\n"
                "Sugiére la cantidad óptima de cada producto a pedir y da una breve explicación de tu razonamiento."
            )
            respuesta = openai_agent.ask_gpt(prompt)
            print(Fore.LIGHTBLUE_EX + "\n--- Sugerencia Óptima de Pedido por IA (ChatGPT) ---\n" + respuesta + Style.RESET_ALL)
        
        elif choice == "10":
            products = db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
            inventory_list = [
                {"nombre": p[0], "cantidad": p[1], "umbral": p[2]}
                for p in products
            ]
            prompt = (
                "Simula una conversacion entre dos agentes:\n"
                "Agente de Inventario: Quiere evitar quiebres de stock y minimizar sobrestock.\n"
                "Agente de compras: quiere minimizar costos y optimizar el presupuestoso.\n"
                f"inventario actual:\n{inventory_list}\n"
                "muestra el dialogo entre ambos agentes para decidir que productos pedir y en que cantidad."
            )
            respuesta = openai_agent.ask_gpt(prompt)
            print(Fore.LIGHTGREEN_EX + "\n--- Simulación de Negociación (IA) ---\n" + respuesta + Style.RESET_ALL)
    
        elif choice == "11":
            products = db_manager.fetch_query("SELECT product_name, quantity, restock_threshold FROM inventory")
            inventory_list = [
                {"nombre": p[0], "cantidad": p[1], "umbral": p[2]}
                for p in products
            ]
            # Ronda 1: Inventario propone
            propuesta_inv = inventory_agent.proponer_pedido(inventory_list)
            print("\n[Agente Inventario]:", propuesta_inv)

            # Ronda 2: Compras responde
            respuesta_compras = compras_agent.responder_a_inventario(propuesta_inv)
            print("\n[Agente Compras]:", respuesta_compras)

            # Ronda 3: Finanzas responde
            respuesta_finanzas = finanzas_agent.responder_a_compras(respuesta_compras)
            print("\n[Agente Finanzas]:", respuesta_finanzas)

            # Resumen final
            prompt_resumen = (
                f"Resume el acuerdo final entre los tres agentes considerando sus objetivos y propuestas:\n"
                f"Inventario: {propuesta_inv}\nCompras: {respuesta_compras}\nFinanzas: {respuesta_finanzas}"
            )
            resumen = openai_agent.ask_gpt(prompt_resumen)
            print(Fore.LIGHTCYAN_EX + "\n--- Acuerdo Final Multi-Agente (MCP-IA) ---\n" + resumen + Style.RESET_ALL)

        elif choice == "12":
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

def agregar_respuesta_ia_a_pdf(respuesta, ruta_pdf):
    pdf = FPDF()
    pdf.add_page()
    pdf.set_font("Arial", size=12)
    pdf.multi_cell(0, 10, respuesta)
    pdf.output(ruta_pdf)

def guardar_respuesta_ia_en_json(respuesta, ruta_json):
    with open(ruta_json, "w", encoding="utf-8") as f:
        json.dump({"reporte_ia": respuesta}, f, ensure_ascii=False, indent=4)

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(f"Error inesperado: {e}")
        with open('data/event_log.txt', 'a') as log_file:
            log_file.write(f"[ERROR] {e}\n")