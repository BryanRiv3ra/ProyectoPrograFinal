import sqlite3

class DBManager:
    def __init__(self, db_file):
        self.db_file = db_file
        self.connection = None

    def connect(self):
        """Establece la conexión a la base de datos SQLite."""
        if self.connection is None:
            self.connection = sqlite3.connect(self.db_file)

    def close(self):
        """Cierra la conexión a la base de datos."""
        if self.connection is not None:
            self.connection.close()
            self.connection = None

    def execute_query(self, query, params=()):
        """Ejecuta una consulta SQL y devuelve los resultados."""
        self.connect()
        cursor = self.connection.cursor()
        cursor.execute(query, params)
        self.connection.commit()
        return cursor.fetchall()

    def execute_insert(self, query, params=()):
        """Ejecuta una consulta de inserción SQL."""
        self.connect()
        cursor = self.connection.cursor()
        cursor.execute(query, params)
        self.connection.commit()
        return cursor.lastrowid

    def fetch_all(self, query, params=()):
        """Recupera todos los registros de una consulta SQL."""
        self.connect()
        cursor = self.connection.cursor()
        cursor.execute(query, params)
        return cursor.fetchall()

    def fetch_one(self, query, params=()):
        """Recupera un único registro de una consulta SQL."""
        self.connect()
        cursor = self.connection.cursor()
        cursor.execute(query, params)
        return cursor.fetchone()

    def initialize_database(self):
        """Inicializa la base de datos y crea las tablas necesarias."""
        connection = self._connect()
        cursor = connection.cursor()

        # Crear tabla de inventario
        cursor.execute("""
        CREATE TABLE IF NOT EXISTS inventory (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            product_name TEXT NOT NULL,
            quantity INTEGER NOT NULL,
            restock_threshold INTEGER NOT NULL
        )
        """)

        # Crear tabla de eventos
        cursor.execute("""
        CREATE TABLE IF NOT EXISTS events (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            event_type TEXT NOT NULL,
            event_description TEXT NOT NULL,
            timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
        )
        """)

        # Crear tabla de cambios en el inventario
        cursor.execute("""
        CREATE TABLE IF NOT EXISTS inventory_changes (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            product_name TEXT NOT NULL,
            change_type TEXT NOT NULL,
            change_description TEXT NOT NULL,
            timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
        )
        """)

        connection.commit()
        connection.close()

    def _connect(self):
        """Establece la conexión con la base de datos."""
        return sqlite3.connect(self.db_file)

    def fetch_query(self, query, params=()):
        """Ejecuta una consulta SELECT y devuelve los resultados."""
        connection = self._connect()
        cursor = connection.cursor()
        cursor.execute(query, params)
        results = cursor.fetchall()
        connection.close()
        return results