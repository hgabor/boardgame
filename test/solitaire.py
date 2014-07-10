#!/usr/bin/env python
# coding: utf-8
from pprint import pprint

from common import *

result = do_request(page='/session', data={'nick': 'Hali'})
nick = result['NickName']
key = result['Key']
port = result['SocketPort']

print("Logged in as '{:s}' with key '{:s}'".format(nick, key))

games = do_request(page='/gametypes')
print('Supported games:', games)

game_data = do_request(page='/games', data={'type': 'pegsolitaire'}, key=key)
game_id = game_data['Id']

game_status = do_request(page='/games/' + game_id, key=key)
pprint(game_status)
