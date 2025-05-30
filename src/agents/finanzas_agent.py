class FinanzasAgent:
    def __init__(self, openai_agent):
        self.openai_agent = openai_agent

    def responder_a_compras(self, respuesta_compras):
        prompt = (
            f"Como Agente de Finanzas, responde a la propuesta del Agente de Compras:\n{respuesta_compras}\n"
            "Tu objetivo es mantener el presupuesto bajo control."
        )
        return self.openai_agent.ask_gpt(prompt)