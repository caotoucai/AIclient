#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

namespace ASAM {
namespace FIBEX {

class FRClusterType;

class ASAM_FIBEX_EXPORT FBCluster
{
public:
    FBCluster();
    ~FBCluster();

    QString name() const;

    const FRClusterType *m_frCluster = nullptr;
    quint32 m_msPerCycle;

};

} // FIBEX
} // ASAM
