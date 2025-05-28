class PredictionAgent:
    def __init__(self, db_manager):
        self.db_manager = db_manager

    def predict_restock(self):
        """
        Predice qué productos necesitan ser reabastecidos basándose en el umbral de reposición.
        """
        query = """
        SELECT product_name, quantity, restock_threshold
        FROM inventory
        WHERE quantity <= restock_threshold
        """
        low_stock_items = self.db_manager.fetch_query(query)

        if not low_stock_items:
            print("No se necesitan reabastecimientos en este momento.")
            return []

        print("Productos que necesitan reabastecimiento:")
        predictions = []
        for product_name, quantity, restock_threshold in low_stock_items:
            restock_amount = restock_threshold * 2  # Ejemplo: duplicar el umbral como predicción
            predictions.append({
                "product_name": product_name,
                "current_quantity": quantity,
                "restock_amount": restock_amount
            })
            print(f"- {product_name}: cantidad actual {quantity}, reabastecer con {restock_amount} unidades.")

        return predictions