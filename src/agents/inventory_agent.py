class InventoryAgent:
    def __init__(self, db_manager, openai_agent):
        self.db_manager = db_manager
        self.openai_agent = openai_agent

    def proponer_pedido(self, inventario):
        prompt = (
            "Como Agente de Inventario, analiza el siguiente inventario y propone qué productos pedir y en qué cantidad "
            "para evitar quiebres y sobrestock:\n"
            f"{inventario}"
        )
        return self.openai_agent.ask_gpt(prompt)