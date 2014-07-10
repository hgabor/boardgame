#!/usr/bin/env python
# coding: utf-8
import unittest
from common import *

__author__ = 'hali'


class ApiErrors(unittest.TestCase):

    def setUp(self):
        data = do_request(page='/session', data={'nick': 'Tester'})
        self.nick = data['NickName']
        self.key = data['Key']
        self.port = data['SocketPort']

    def tearDown(self):
        pass

    def test_no_session(self):
        data = do_request('/session')
        self.assertError(data, 1)

    def test_invalid_key(self):
        data = do_request('/session', key='1234-abcd')
        self.assertError(data, 2)

    def assertError(self, data, errorCode):
        self.assertTrue('error' in data, 'No "error" key in data')
        self.assertTrue('errorCode' in data, 'No "errorCode" key in data')
        self.assertTrue(data['errorCode'] == errorCode, 'errorCode should be ' + str(errorCode))

if __name__ == '__main__':
    unittest.main()
