class InventoryAgent:
    def __init__(self, db_manager):
        self.db_manager = db_manager

    def add_product(self, product_name, quantity, restock_threshold=10):
        """Agrega un nuevo producto al inventario."""
        if not product_name or not isinstance(product_name, str):
            raise ValueError("El nombre del producto debe ser una cadena no vacía.")
        if quantity < 0 or restock_threshold < 0:
            raise ValueError("La cantidad y el umbral de reposición deben ser mayores o iguales a 0.")
        query = """
        INSERT INTO inventory (product_name, quantity, restock_threshold)
        VALUES (?, ?, ?)
        """
        self.db_manager.execute_query(query, (product_name, quantity, restock_threshold))
        print(f"Producto '{product_name}' agregado con cantidad {quantity}.")

    def update_product(self, product_name, new_quantity):
        """Actualiza la cantidad de un producto existente."""
        query = """
        UPDATE inventory
        SET quantity = ?
        WHERE product_name = ?
        """
        self.db_manager.execute_query(query, (new_quantity, product_name))
        print(f"Producto '{product_name}' actualizado a cantidad {new_quantity}.")

    def check_inventory(self):
        """Verifica el inventario y muestra los productos que necesitan reposición."""
        query = """
        SELECT product_name, quantity, restock_threshold
        FROM inventory
        WHERE quantity <= restock_threshold
        """
        results = self.db_manager.fetch_query(query)
        if results:
            print("Productos que necesitan reposición:")
            for product_name, quantity, restock_threshold in results:
                print(f"- {product_name}: {quantity} unidades (umbral: {restock_threshold})")
        else:
            print("Todos los productos están por encima del umbral de reposición.")
    
    def delete_product(self, product_name):
        """Elimina un producto del inventario."""
        query = "DELETE FROM inventory WHERE product_name = ?"
        self.db_manager.execute_query(query, (product_name,))
        print(f"Producto '{product_name}' eliminado del inventario.")

    def search_product(self, product_name):
        """Busca un producto en el inventario por su nombre."""
        query = "SELECT * FROM inventory WHERE product_name LIKE ?"
        results = self.db_manager.fetch_query(query, (f"%{product_name}%",))
        return results