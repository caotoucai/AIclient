#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXPduInstanceType.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType PDU-INSTANCES
 *
 * A frames layout as a sequence of PDUs
 */
class ASAM_FIBEX_EXPORT FXPduInstances
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXPduInstances(FXFibex *fibex = nullptr);
    ~FXPduInstances();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element PDU-INSTANCE */
    QHash<QString, FXPduInstanceType*> m_pduInstances;

};

} // FIBEX
} // ASAM
