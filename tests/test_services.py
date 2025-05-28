import unittest
from src.services.event_logger import EventLogger
from src.services.report_generator import ReportGenerator

class TestServices(unittest.TestCase):

    def setUp(self):
        self.event_logger = EventLogger()
        self.report_generator = ReportGenerator()

    def test_event_logging(self):
        self.event_logger.log_event("Test event")
        self.assertIn("Test event", self.event_logger.events)

    def test_report_generation_json(self):
        report = self.report_generator.generate_report(format='json')
        self.assertIsInstance(report, dict)

    def test_report_generation_pdf(self):
        report = self.report_generator.generate_report(format='pdf')
        self.assertIsInstance(report, bytes)

if __name__ == '__main__':
    unittest.main()