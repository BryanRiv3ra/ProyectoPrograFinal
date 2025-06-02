# Sistema de Inventario Inteligente con IA, A2A y MCP

## Descripción

Este proyecto es un sistema de inventario inteligente que permite gestionar productos, generar reportes avanzados y simular negociaciones entre agentes usando inteligencia artificial (OpenAI/ChatGPT).  
Incluye simulaciones de negociación entre agentes (A2A) y una plataforma de control multi-agente (MCP) para la toma de decisiones colaborativa.

---

## Características principales

- **Gestión de inventario**: Agregar, actualizar, eliminar y consultar productos.
- **Reportes automáticos**: Generación de reportes en PDF y JSON.
- **Predicción de reabastecimiento**: Sugerencias inteligentes usando IA.
- **Clasificación ABC y reportes ejecutivos**: Análisis avanzado con ChatGPT.
- **Simulación A2A**: Negociación entre dos agentes (Inventario y Compras).
- **Simulación MCP**: Negociación multi-agente (Inventario, Compras y Finanzas).
- **Logs y manejo de errores**: Registro de eventos y errores para auditoría.

---

## Estructura del proyecto

```
Proyecto Final/
│
├── src/
│   ├── main.py
│   ├── agents/
│   │   ├── inventory_agent.py
│   │   ├── compras_agent.py
│   │   ├── finanzas_agent.py
│   │   └── openai_agent.py
│   ├── services/
│   │   ├── event_logger.py
│   │   └── report_generator.py
│   ├── database/
│   │   └── db_manager.py
│
├── data/
│   ├── inventory.db
│   ├── *.pdf, *.json, event_log.txt
│
├── tests/
│   └── test_services.py
│
└── README.md
```

---

## Instalación

1. **Clona el repositorio**  
   ```bash
   git clone <URL-del-repo>
   cd "Proyecto Final"
   ```

2. **Instala las dependencias**  
   Asegúrate de tener Python 3.8+ y ejecuta:
   ```bash
   pip install openai fpdf tabulate
   ```

3. **Configura tu clave de OpenAI**  
   Abre `src/main.py` y coloca tu clave de API de OpenAI en la variable `openai_api_key`.

---

## Uso

1. **Ejecuta el sistema desde la raíz del proyecto:**
   ```bash
   python src/main.py
   ```

2. **Sigue el menú interactivo** para gestionar productos, generar reportes o simular negociaciones inteligentes.

---

## Integración de OpenAI

El sistema utiliza la API de OpenAI (ChatGPT) para:
- Analizar el inventario y sugerir pedidos óptimos.
- Generar reportes ejecutivos y clasificaciones ABC.
- Simular negociaciones entre agentes (A2A y MCP).

La integración se realiza a través del archivo `src/agents/openai_agent.py`, que encapsula las llamadas a la API y permite enviar prompts personalizados según el contexto.

---

## Simulación A2A (Agent-to-Agent)

La opción 10 del menú ejecuta una simulación de negociación entre dos agentes:
- **Agente de Inventario:** Busca evitar quiebres y sobrestock.
- **Agente de Compras:** Busca minimizar costos y optimizar el presupuesto.

La IA genera un diálogo entre ambos agentes para decidir qué productos pedir y en qué cantidad.

---

## Simulación MCP (Multi-Agent Control Platform)

La opción 11 del menú ejecuta una simulación multi-agente:
- **Agente de Inventario:** Propone pedidos.
- **Agente de Compras:** Responde buscando minimizar costos.
- **Agente de Finanzas:** Responde cuidando el presupuesto.

Cada agente responde de forma independiente y la IA resume el acuerdo final, mostrando cómo se llega a una decisión colaborativa.

---

## Pruebas

El proyecto incluye pruebas unitarias en la carpeta `tests/` para asegurar el correcto funcionamiento de los servicios principales.

Ejecuta las pruebas con:
```bash
python -m unittest discover tests
```
---
## Créditos
- Basado en tecnologías de Python, OpenAI y FPDF.
---

