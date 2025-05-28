import unittest
from src.agents.inventory_agent import InventoryAgent
from src.agents.prediction_agent import PredictionAgent

class TestInventoryAgent(unittest.TestCase):

    def setUp(self):
        self.agent = InventoryAgent()

    def test_add_product(self):
        self.agent.add_product("Product A", 10)
        self.assertIn("Product A", self.agent.inventory)
        self.assertEqual(self.agent.inventory["Product A"], 10)

    def test_remove_product(self):
        self.agent.add_product("Product B", 5)
        self.agent.remove_product("Product B")
        self.assertNotIn("Product B", self.agent.inventory)

    def test_update_product(self):
        self.agent.add_product("Product C", 20)
        self.agent.update_product("Product C", 15)
        self.assertEqual(self.agent.inventory["Product C"], 15)

class TestPredictionAgent(unittest.TestCase):

    def setUp(self):
        self.prediction_agent = PredictionAgent()

    def test_predict_demand(self):
        self.prediction_agent.add_sales_data("Product A", [10, 12, 15])
        prediction = self.prediction_agent.predict_demand("Product A")
        self.assertIsInstance(prediction, float)

if __name__ == '__main__':
    unittest.main()