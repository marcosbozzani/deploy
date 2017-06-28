#!/bin/bash -e

destination=/apps/bin/centraldasorte/sorteios.proxy/

rm -rf $destination
mkdir -p $destination
tar -xzvf package.tar.gz -C $destination
