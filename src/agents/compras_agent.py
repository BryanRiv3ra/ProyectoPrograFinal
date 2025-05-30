class ComprasAgent:
    def __init__(self, openai_agent):
        self.openai_agent = openai_agent

    def responder_a_inventario(self, propuesta_inventario):
        prompt = (
            f"Como Agente de Compras, responde a la propuesta del Agente de Inventario:\n{propuesta_inventario}\n"
            "Tu objetivo es minimizar costos y negociar con proveedores."
        )
        return self.openai_agent.ask_gpt(prompt)