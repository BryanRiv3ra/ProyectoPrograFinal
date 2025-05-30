import openai

class OpenAIAgent:
    def __init__(self, api_key):
        self.client = openai.OpenAI(api_key=api_key)

    def ask_gpt(self, prompt):
        response = self.client.chat.completions.create(
            model="gpt-3.5-turbo",
            messages=[
                {"role": "system", "content": "Eres un asistente experto en gestión de inventarios."},
                {"role": "user", "content": prompt}
            ]
        )
        return response.choices[0].message.content.strip()