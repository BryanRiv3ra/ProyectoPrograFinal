def validate_product_data(product_data):
    # Validates the product data to ensure it contains necessary fields
    required_fields = ['id', 'name', 'quantity', 'price']
    for field in required_fields:
        if field not in product_data:
            raise ValueError(f"Missing required field: {field}")
    return True

def format_currency(amount):
    # Formats a number as currency
    return "${:,.2f}".format(amount)

def calculate_reorder_level(current_stock, safety_stock):
    # Calculates the reorder level based on current stock and safety stock
    return current_stock <= safety_stock

def parse_json_file(file_path):
    # Parses a JSON file and returns the data
    import json
    with open(file_path, 'r') as file:
        return json.load(file)