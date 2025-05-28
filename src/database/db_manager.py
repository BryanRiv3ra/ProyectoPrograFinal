import sqlite3

class DBManager:
    def __init__(self, db_path):
        self.db_path = db_path

    def _connect(self):
        try:
            connection = sqlite3.connect(self.db_path)
            connection.execute("PRAGMA integrity_check;")
            return connection
        except sqlite3.DatabaseError as e:
            print(f"Error: El archivo no es una base de datos válida. {e}")
            raise

    def initialize_database(self):
        connection = self._connect()
        cursor = connection.cursor()
        cursor.execute("""
            CREATE TABLE IF NOT EXISTS inventory (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                product_name TEXT NOT NULL,
                quantity INTEGER NOT NULL,
                restock_threshold INTEGER NOT NULL
            )
        """)
        connection.commit()
        connection.close()

    def execute_query(self, query, params=()):
        connection = self._connect()
        cursor = connection.cursor()
        cursor.execute(query, params)
        connection.commit()
        connection.close()

    def fetch_query(self, query, params=()):
        connection = self._connect()
        cursor = connection.cursor()
        cursor.execute(query, params)
        results = cursor.fetchall()
        connection.close()
        return results