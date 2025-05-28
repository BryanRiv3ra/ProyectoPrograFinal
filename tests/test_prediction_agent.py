import unittest
from database.db_manager import DBManager
from agents.prediction_agent import PredictionAgent

class TestPredictionAgent(unittest.TestCase):
    def setUp(self):
        self.db_manager = DBManager(':memory:')  # Base de datos en memoria
        self.db_manager.initialize_database()
        self.prediction_agent = PredictionAgent(self.db_manager)

    def test_predict_restock(self):
        self.db_manager.execute_query(
            "INSERT INTO inventory (product_name, quantity, restock_threshold) VALUES (?, ?, ?)",
            ('Producto Test', 5, 10)
        )
        predictions = self.prediction_agent.predict_restock()
        self.assertEqual(len(predictions), 1)
        self.assertEqual(predictions[0]['product_name'], 'Producto Test')

if __name__ == '__main__':
    unittest.main()