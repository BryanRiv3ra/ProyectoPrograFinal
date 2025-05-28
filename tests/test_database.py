import unittest
from src.database.db_manager import DBManager

class TestDBManager(unittest.TestCase):

    @classmethod
    def setUpClass(cls):
        cls.db_manager = DBManager('data/inventory.db')
        cls.db_manager.create_table()  # Assuming there's a method to create the necessary table

    def test_insert_item(self):
        item = {'name': 'Test Item', 'quantity': 10}
        result = self.db_manager.insert_item(item)
        self.assertTrue(result)

    def test_get_item(self):
        item_name = 'Test Item'
        item = self.db_manager.get_item(item_name)
        self.assertIsNotNone(item)
        self.assertEqual(item['name'], item_name)

    def test_update_item(self):
        item_name = 'Test Item'
        new_quantity = 20
        result = self.db_manager.update_item(item_name, new_quantity)
        self.assertTrue(result)

        updated_item = self.db_manager.get_item(item_name)
        self.assertEqual(updated_item['quantity'], new_quantity)

    def test_delete_item(self):
        item_name = 'Test Item'
        result = self.db_manager.delete_item(item_name)
        self.assertTrue(result)

        deleted_item = self.db_manager.get_item(item_name)
        self.assertIsNone(deleted_item)

    @classmethod
    def tearDownClass(cls):
        cls.db_manager.close_connection()  # Assuming there's a method to close the database connection

if __name__ == '__main__':
    unittest.main()